using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Linq.AI.Microsoft
{

    /// <summary>
    /// ITransformer for Microsoft.Extensions.AI IChatClient
    /// </summary>
    public class MicrosoftChatClientTransformer : ITransformer
    {
        private IChatClient _chatClient;

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
        /// <param name="client">The Microsoft.Extensions.AI.IChatClient to use.</param>
        public MicrosoftChatClientTransformer(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        /// <summary>
        /// Temperature to use.
        /// </summary>
        public float? Temperature { get; set; } = 0.0f;

        /// <summary>
        /// Tool registrations
        /// </summary>
        [JsonIgnore]
        public IList<AITool> Tools { get; set; } = new List<AITool>();

        /// <summary>
        /// Generate an item of shape T based on "goal"
        /// </summary>
        /// <typeparam name="T">type of items</typeparam>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>collection of items which didn't match the goal</returns>
        public ValueTask<ResultT> GenerateAsync<ResultT>(string goal, string? instructions = null, CancellationToken cancellationToken = default)
            => TransformItemAsync<ResultT>(null, goal, instructions, cancellationToken);


        /// <summary>
        /// Transform item using OpenAI model
        /// </summary>
        /// <param name="item">item to Transform</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>transformed text</returns>
        public async ValueTask<ResultT> TransformItemAsync<ResultT>(object item, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<Transformation<ResultT>>().ToString();
            var schemaElement = JsonDocument.Parse(schema).RootElement;
            var responseFormat = ChatResponseFormat.ForJsonSchema(schemaElement);
            var context = new CompletionContext()
            {
                Item = item,
                Options = new ChatOptions()
                {
                    ResponseFormat = responseFormat,
                    Temperature = this.Temperature,
                    AllowMultipleToolCalls = Tools.Any() ? true : null,
                    Tools = this.Tools
                }
            };

            context.Messages.Add(GetTransformerSystemPrompt(goal ?? "Transform", item != null, instructions));
            if (item != null)
                context.Messages.Add(GetTransformerItemMessage(item));

            Debug.WriteLine("===============================================");
#if DEBUG
            lock (this)
            {
                foreach (var message in context.Messages)
                {
                    foreach (var part in message.Contents)
                        Debug.WriteLine(JsonConvert.SerializeObject(part, JsonSettings));
                }
            }
#endif
            int retries = 2;
            while (retries-- > 0)
            {
                context.Result = await _chatClient.GetResponseAsync(context.Messages, context.Options, cancellationToken: cancellationToken);

                if (context.Completion == ChatFinishReason.Stop)
                {

                    // Add the assistant message to the conversation history.
                    // messages.Add(new AssistantChatMessage(completion));
#if DEBUG
                    lock (this)
                    {
                        Debug.WriteLine(context.Result.Text);
                    }
#endif
                    var transformation = JsonConvert.DeserializeObject<Transformation<ResultT>>(context.Result.Text, JsonSettings)!;
                    return transformation.Result!;
                }
                else if (context.Completion == ChatFinishReason.ToolCalls)
                {
                    throw new NotImplementedException("Use FunctionInvokingChatClient: to support functions.");
                }
                else if (context.Completion == ChatFinishReason.Length)
                {
                    throw new NotImplementedException("Incomplete model output due to MaxTokens parameter or token limit exceeded.");
                }
                else if (context.Completion == ChatFinishReason.ContentFilter)
                {
                    throw new NotImplementedException("Omitted content due to a content filter flag.");
                }
                else
                {
                    throw new NotImplementedException(context.Completion.Value.ToString());
                }
            }

            throw new Exception("Too many function calls detected!");
        }

        /// <summary>
        /// Transform items 
        /// </summary>
        /// <typeparam name="ResultT"></typeparam>
        /// <param name="source"></param>
        /// <param name="goal"></param>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public IAsyncEnumerable<ResultT> TransformItemsAsync<ResultT>(IEnumerable<object> source, string? goal = null, string? instructions = null)
            => this.TransformItemsAsync<ResultT>(source.ToAsyncEnumerable(), goal, instructions);

        /// <summary>
        /// Transform items using AI model
        /// </summary>
        /// <typeparam name="ResultT">result type</typeparam>
        /// <param name="source">source collection</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>transformed results</returns>
        public IAsyncEnumerable<ResultT> TransformItemsAsync<ResultT>(IAsyncEnumerable<object> source, string? goal = null, string? instructions = null)
        {
            var schema = StructuredSchemaGenerator.FromType<Transformation<ResultT>>().ToString();

            return source.SelectAwaitWithCancellation((item, index, ct) =>
                    TransformItemAsync<ResultT>(item, goal, Utils.GetItemIndexClause((int)index, instructions), ct));
        }

        /// <summary>
        /// Add all static methods on a class as tools.
        /// </summary>
        /// <typeparam name="ToolClassT"></typeparam>
        /// <returns></returns>
        public MicrosoftChatClientTransformer AddFunctions<ToolClassT>()
            where ToolClassT : class
        {
            foreach (var method in typeof(ToolClassT).GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                var paramTypes = method.GetParameters().Select(p => p.ParameterType).ToList();
                paramTypes.Add(method.ReturnType);

                Type delegateType = Expression.GetDelegateType(paramTypes.ToArray());
                string description = method.GetCustomAttribute<DescriptionAttribute>()?.Description ??
                        method.GetCustomAttribute<InstructionAttribute>()?.Instruction
                        ?? method.Name;
                // var priorityGroup = method.GetCustomAttribute<PriorityGroupAttribute>()?.Group ?? 0;
                AddFunction(method.Name, description, Delegate.CreateDelegate(delegateType, method, true)!);// , priorityGroup);
            }
            return this;
        }

        /// <summary>
        /// Add Static Method as a tool.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public MicrosoftChatClientTransformer AddFunction(string name, string description, Delegate del)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(description);
            ArgumentNullException.ThrowIfNull(del);
            this.Tools.Add(AIFunctionFactory.Create(del, new AIFunctionFactoryOptions() { Name = name, Description = description }));
            return this;
        }

        internal static ChatMessage GetTransformerSystemPrompt(string goal, bool hasItem, string? instructions = null)
        {
            var transformText = hasItem ? "Transform <ITEM> using the directions in the provided <GOAL>." : "";
            return new ChatMessage(ChatRole.System,
                    $$"""
                    You are an expert at transforming.

                    <GOAL>
                    {{goal}}

                    <INSTRUCTIONS>
                    {{transformText}}
                    {{instructions}}
                    """);
        }

        internal static ChatMessage GetTransformerItemMessage(object item)
        {
            if (item is string)
            {
                return new ChatMessage(ChatRole.User,
                    $$"""
                    <ITEM>
                    {{item}}
                    """);
            }
            else if (item is AIContent contentPart)
            {
                return new ChatMessage(ChatRole.User, new List<AIContent>()
                    {
                        new TextContent("<ITEM>"),
                        contentPart
                    });
            }
            else if (item is AIContent[] contentParts)
            {
                return new ChatMessage(ChatRole.User, contentParts);
            }
            else if (item is Uri uri)
            {
                return new ChatMessage(ChatRole.User, new List<AIContent>()
                {
                    new TextContent("<ITEM>"),
                    CreateUriContent(uri)
                });
            }
            else if (item is Uri[] uris)
            {
                List<AIContent> parts = new List<AIContent>()
                {
                    new TextContent("<ITEM>"),
                };

                foreach (var u in uris)
                {
                    parts.Add(CreateUriContent(u));
                }
                return new ChatMessage(ChatRole.User, parts);
            }
            else
            {
                return new ChatMessage(ChatRole.User,
                    $$"""
                    <ITEM>
                    {{JToken.FromObject(item).ToString()}}
                    """);
            }
        }

        private static UriContent CreateUriContent(Uri uri)
        {
            if (MimeTypes.TryGetMimeType(Path.GetExtension(uri.AbsoluteUri), out var mimeType))
            {
                return new UriContent(uri.AbsoluteUri, mimeType);
            }
            return new UriContent(uri.AbsoluteUri, "application/octet-stream");
        }
    }
}
