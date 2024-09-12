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
        /// Transform collection of text into text using OpenAI model
        /// </summary>
        /// <param name="source">collection of text</param>
        /// <param name="model">ChatClient model</param>
        /// <param name="goal">The goal describing the transformation desired</param>
        /// <param name="instructions">(OPTIONAL) extend system instructions</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel tasks running queries</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>collection of transformed text</returns>
        public static IList<string> Select(this IEnumerable<string> source, ChatClient model, string goal, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.Select<string, string>(model, goal, instructions, maxParallel, cancellationToken);
        }

        /// <summary>
        /// Transform collection of text into objects using OpenAI model
        /// </summary>
        /// <typeparam name="ResultT">the type of result</typeparam>
        /// <param name="source">collection of text</param>
        /// <param name="model">ChatClient model</param>
        /// <param name="goal">(OPTIONAL) The goal describing the transformation desired</param>
        /// <param name="instructions">(OPTIONAL) extend system instructions</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel tasks running queries</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>collection of generated objects</returns>
        public static IList<ResultT> Select<ResultT>(this IEnumerable<string> source, ChatClient model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.Select<string, ResultT>(model, goal, instructions, maxParallel, cancellationToken);
        }

        /// <summary>
        /// Transform collection of objects into objects using OpenAI model
        /// </summary>
        /// <typeparam name="SourceT">the type of source</typeparam>
        /// <typeparam name="ResultT">the type of result</typeparam>
        /// <param name="source">collection of text</param>
        /// <param name="model">ChatClient model</param>
        /// <param name="goal">(OPTIONAL) The goal describing the transformation desired</param>
        /// <param name="instructions">(OPTIONAL) extend system instructions</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel tasks running queries</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>collection of transformed objects</returns>
        public static IList<ResultT> Select<SourceT, ResultT>(this IEnumerable<SourceT> source, ChatClient model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<SelectItem<ResultT>>().ToString();

            var count = source.Count();

            return source.SelectParallelAsync(async (item, index, ct) =>
            {
                var itemResult = item;
                var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "transform", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
                ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
                var systemChatMessage = GetSystemPrompt(goal ?? "transform the item to the output schema", schema, instructions);
                var itemMessage = Utils.GetItemPrompt(itemResult!, index, count);
                ChatCompletion chatCompletion = await model.CompleteChatAsync([systemChatMessage, itemMessage], options, ct);
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
            }, maxParallel: maxParallel ?? Environment.ProcessorCount * 2, cancellationToken: cancellationToken);
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

