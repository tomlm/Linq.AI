using Iciclecreek.Async;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;
using System.Diagnostics;

namespace Linq.AI.OpenAI
{

    /// <summary>
    /// ITransformer for OpenAI
    /// </summary>
    public class OpenAITransformer : ITransformer
    {
        private ChatClient _chatClient;

        internal class Transformation<T>
        {
            [Description("Explain your reasoning")]
            public string? Explanation { get; set; }

            [Description("The result of the goal")]
            public T? Result { get; set; }
        }

        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        /// <summary> Initializes a new instance of <see cref="OpenAITransformer">. </summary>
        /// <param name="model"> The name of the model to use in requests sent to the service. To learn more about the available models, see <see href="https://platform.openai.com/docs/models"/>. </param>
        /// <param name="credential"> The API key to authenticate with the service. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="model"/> or <paramref name="credential"/> is null. </exception>
        /// <exception cref="ArgumentException"> <paramref name="model"/> is an empty string, and was expected to be non-empty. </exception>
        public OpenAITransformer(string model, ApiKeyCredential credential) : this(model, credential, new OpenAIClientOptions())
        {
        }

        /// <summary> Initializes a new instance of <see cref="OpenAITransformer">. </summary>
        /// <param name="model"> The name of the model to use in requests sent to the service. To learn more about the available models, see <see href="https://platform.openai.com/docs/models"/>. </param>
        /// <param name="credential"> The API key to authenticate with the service. </param>
        /// <param name="options"> The options to configure the client. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="model"/> or <paramref name="credential"/> is null. </exception>
        /// <exception cref="ArgumentException"> <paramref name="model"/> is an empty string, and was expected to be non-empty. </exception>
        public OpenAITransformer(string model, ApiKeyCredential credential, OpenAIClientOptions options) : this(new ChatClient(model, credential, options))
        {
        }

        public OpenAITransformer(ChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public float? Temperature { get; set; } = 0.0f;

        /// <summary>
        /// Generate an item of shape T based on "goal"
        /// </summary>
        /// <typeparam name="ResultT">type of items</typeparam>
        /// <param name="source">collection of items</param>
        /// <param name="constraint">The constraint to match to remove an item</param>
        /// <param name="instructions">(OPTIONAL) additional instructions</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel operations</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>collection of items which didn't match the goal</returns>
        public ResultT Generate<ResultT>(string goal, string? instructions = null, CancellationToken cancellationToken = default)
            => TransformItem<ResultT>(String.Empty, goal, instructions, cancellationToken);

        /// <summary>
        /// Generate an item of shape T based on "goal"
        /// </summary>
        /// <typeparam name="T">type of items</typeparam>
        /// <param name="source">collection of items</param>
        /// <param name="constraint">The constraint to match to remove an item</param>
        /// <param name="instructions">(OPTIONAL) additional instructions</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel operations</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>collection of items which didn't match the goal</returns>
        public Task<ResultT> GenerateAsync<ResultT>(string goal, string? instructions = null, CancellationToken cancellationToken = default)
            => TransformItemAsync<ResultT>(String.Empty, goal, instructions, cancellationToken);

        /// <summary>
        /// Transform text using OpenAI model
        /// </summary>
        /// <param name="item">item to Transform</param>
        /// <param name="model">ChatClient to use as model</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>transformed text</returns>
        public ResultT TransformItem<ResultT>(object item, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
            => TransformItemAsync<ResultT>(item, goal, instructions, cancellationToken).Result;

        /// <summary>
        /// Transform item using OpenAI model
        /// </summary>
        /// <param name="item">item to Transform</param>
        /// <param name="model">ChatClient to use as model</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>transformed text</returns>
        public async Task<ResultT> TransformItemAsync<ResultT>(object item, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<Transformation<ResultT>>().ToString();
            var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "Transform", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
            ChatCompletionOptions options = new ChatCompletionOptions() 
            {
                ResponseFormat = responseFormat, 
                Temperature = this.Temperature 
            };
            var systemChatMessage = GetSystemPrompt(goal ?? "Transform", instructions);
            var itemMessage = GetItemMessage(item);
            ChatCompletion chatCompletion = await _chatClient.CompleteChatAsync([systemChatMessage, itemMessage], options, cancellationToken: cancellationToken);
            return chatCompletion.Content.Select(completion =>
            {
#if DEBUG
                lock (this)
                {
                    Debug.WriteLine("===============================================");
                    Debug.WriteLine(systemChatMessage.Content.Single().Text);
                    Debug.WriteLine(itemMessage.Content.Single().Text);
                    Debug.WriteLine(completion.Text);
                }
#endif
                var transformation = JsonConvert.DeserializeObject<Transformation<ResultT>>(completion.Text, JsonSettings)!;
                return transformation.Result!;
            }).Single()!;
        }

        /// <summary>
        /// Transform items using pool of background tasks and AI model
        /// </summary>
        /// <typeparam name="ResultT">result type</typeparam>
        /// <param name="source">source collection</param>
        /// <param name="model">ai model to use</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel operations</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>transformed results</returns>
        public IList<ResultT> TransformItems<ResultT>(IEnumerable<object> source, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<Transformation<ResultT>>().ToString();
            var count = source.Count();

            return source.SelectParallelAsync(async (item, index, ct) =>
            {
                var itemResult = item;
                var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "transform", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
                ChatCompletionOptions options = new ChatCompletionOptions()
                {
                    ResponseFormat = responseFormat,
                    Temperature = this.Temperature
                };
                var systemChatMessage = GetSystemPrompt(goal ?? "transform the item to the output schema", instructions!, index, count);
                var itemMessage = GetItemMessage(itemResult!);
                ChatCompletion chatCompletion = await _chatClient.CompleteChatAsync([systemChatMessage, itemMessage], options, ct);
                return chatCompletion.Content.Select(completion =>
                {
#if DEBUG
                    lock (this)
                    {
                        Debug.WriteLine("===============================================");
                        Debug.WriteLine(systemChatMessage.Content.Single().Text);
                        Debug.WriteLine(itemMessage.Content.Single().Text);
                        Debug.WriteLine(completion.Text);
                    }
#endif
                    var transformation = JsonConvert.DeserializeObject<Transformation<ResultT>>(completion.Text, JsonSettings)!;
                    return transformation.Result;
                }).Single()!;
            }, maxParallel: maxParallel ?? 2 * Environment.ProcessorCount, cancellationToken);
        }

        internal static SystemChatMessage GetSystemPrompt(string goal, string? instructions = null, int? index = null, int? count = null)
        {
            if (index != null && count != null)
            {
                instructions = Utils.GetItemIndexClause((int)index, (int)count, instructions);
            }
            return new SystemChatMessage($$"""
                    You are an expert at transforming text.

                    <GOAL>
                    {{goal}}

                    <INSTRUCTIONS>
                    Given <ITEM> text transform the text using the directions in the provided <GOAL>.
                    {{instructions}}
                    """);
        }

        internal static UserChatMessage GetItemMessage(object item)
        {
            if (!(item is string))
            {
                item = JToken.FromObject(item).ToString();
            }
            return new UserChatMessage($$"""
            <ITEM>
            {{item}}
            """);
        }

    }
}
