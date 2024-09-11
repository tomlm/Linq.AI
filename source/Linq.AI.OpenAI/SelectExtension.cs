using Iciclecreek.Async;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OpenAI.Chat;
using System.Diagnostics;

namespace Linq.AI.OpenAI
{
    internal class SelectItem<ResultT>
    {
        public string? Explanation { get; set; }

        public ResultT? Result { get; set; }
    }

    public static class SelectExtension
    {
        /// <summary>
        /// Use LLM to Transform text to text
        /// </summary>
        /// <param name="source"></param>
        /// <param name="chatClient"></param>
        /// <param name="goal"></param>
        /// <param name="instructions"></param>
        /// <param name="maxParallel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static IEnumerable<string> SelectAI(this IEnumerable<string> source, ChatClient chatClient, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.SelectAI<string, SelectItem<string>>(chatClient, goal, instructions, maxParallel, cancellationToken)
                        .Select(s => s.Result!);
        }

        /// <summary>
        /// Use LLM to Transform text to object
        /// </summary>
        /// <typeparam name="ResultT"></typeparam>
        /// <param name="source"></param>
        /// <param name="chatClient"></param>
        /// <param name="goal"></param>
        /// <param name="instructions"></param>
        /// <param name="maxParallel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static IEnumerable<ResultT> SelectAI<ResultT>(this IEnumerable<string> source, ChatClient chatClient, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.SelectAI<string, ResultT>(chatClient, goal, instructions, maxParallel, cancellationToken);
        }

        /// <summary>
        /// Use LLM to Transform each object to object 
        /// </summary>
        /// <typeparam name="SourceT"></typeparam>
        /// <param name="source"></param>
        /// <param name="goal">The goal</param>
        /// <param name="categories">the categories</param>
        /// <param name="instructions">additional instructions</param>
        /// <param name="maxParallel">parallezation</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns></returns>
        public static IEnumerable<ResultT> SelectAI<SourceT, ResultT>(this IEnumerable<SourceT> source, ChatClient chatClient, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<ResultT>().ToString();

            var count = source.Count();

            return source.SelectParallelAsync(async (item, index) =>
            {
                var itemResult = item;
                var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "transform", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
                ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
                var systemChatMessage = GetSystemPrompt(goal ?? "transform the item to the output schema", schema, instructions);
                var itemMessage = GetItemPrompt(itemResult!, index, count);
                ChatCompletion chatCompletion = await chatClient.CompleteChatAsync([systemChatMessage, itemMessage], options);
                return chatCompletion.Content.Select(completion =>
                {
                    if (Debugger.IsAttached)
                    {

                        lock (source)
                        {
                            Debug.WriteLine("===============================================");
                            Debug.WriteLine(systemChatMessage.Content.Single().Text);
                            Debug.WriteLine(itemMessage.Content.Single().Text);
                            Debug.WriteLine(completion.Text);
                        }
                    }
                    return JsonConvert.DeserializeObject<ResultT>(completion.Text)!;
                }).Single()!;
            }, maxParallel: maxParallel ?? int.MaxValue);
        }

        private static SystemChatMessage GetSystemPrompt(string goal, string schema, string? instructions = null)
        {
            return new SystemChatMessage($$"""
                    You are an expert at mapping list items from one type to another.

                    <GOAL>
                    {{goal}} 

                    <INSTRUCTIONS>
                    Given an <ITEM> return a new JSON <OUTPUT> object that maps the item to the shape specified by the <GOAL>.{{instructions}}

                    <OUTPUT>
                    {{schema}}
                    """);
        }

        private static UserChatMessage GetItemPrompt(object item, int index, int length)
        {
            if (!(item is string))
            {
                item = JToken.FromObject(item).ToString();
            }
            return new UserChatMessage($$"""
            <INDEX>
            {{index}} of {{length}}

            <ITEM>
            {{item}}
            """);
        }
    }
}

