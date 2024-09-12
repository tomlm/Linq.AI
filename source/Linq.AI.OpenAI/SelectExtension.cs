using Iciclecreek.Async;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OpenAI.Chat;
using System.Diagnostics;
using System.ComponentModel;

namespace Linq.AI.OpenAI
{
    internal class SelectItem<ResultT>
    {
        public string? Explanation { get; set; }

        [Description("The result of the goal")]
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
        public static IList<string> Select(this IEnumerable<string> source, ChatClient model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.Select<string, string>(model, goal, instructions, maxParallel, cancellationToken);
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
        public static IList<ResultT> Select<ResultT>(this IEnumerable<string> source, ChatClient model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.Select<string, ResultT>(model, goal, instructions, maxParallel, cancellationToken);
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
        public static IList<ResultT> Select<SourceT, ResultT>(this IEnumerable<SourceT> source, ChatClient model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<SelectItem<ResultT>>().ToString();

            var count = source.Count();

            return source.SelectParallelAsync(async (item, index) =>
            {
                var itemResult = item;
                var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "transform", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
                ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
                var systemChatMessage = GetSystemPrompt(goal ?? "transform the item to the output schema", schema, instructions);
                var itemMessage = Utils.GetItemPrompt(itemResult!, index, count);
                ChatCompletion chatCompletion = await model.CompleteChatAsync([systemChatMessage, itemMessage], options);
                return chatCompletion.Content.Select(completion =>
                {
#if DEBUG
                    lock (source)
                    {
                        Debug.WriteLine("===============================================");
                        Debug.WriteLine(systemChatMessage.Content.Single().Text);
                        Debug.WriteLine(itemMessage.Content.Single().Text);
                        Debug.WriteLine(completion.Text);
                    }
#endif
                    return JsonConvert.DeserializeObject<SelectItem<ResultT>>(completion.Text)!.Result;
                }).Single()!;
            }, maxParallel: maxParallel ?? Environment.ProcessorCount * 2);
        }

        private static SystemChatMessage GetSystemPrompt(string goal, string schema, string? instructions = null)
        {
            return new SystemChatMessage($$"""
                    You are an expert at mapping list items from one type to another.

                    <GOAL>
                    {{goal}} 

                    <INSTRUCTIONS>
                    Given an <ITEM> return a new JSON <OUTPUT> object that maps the item to the shape specified by the <GOAL>.
                    The item index starts at 0.
                    {{instructions ?? String.Empty}}

                    """);
        }

    }
}

