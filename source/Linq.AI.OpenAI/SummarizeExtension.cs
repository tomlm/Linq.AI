using Iciclecreek.Async;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OpenAI.Chat;
using System.Diagnostics;

namespace Linq.AI.OpenAI
{

    internal class Summarization
    {
        public string? Explanation { get; set; }

        public string? Summary { get; set; }
    }

    public static class SummarizeExtension
    {
        /// <summary>
        /// Summarize text using OpenAI model
        /// </summary>
        /// <param name="text">text to summarize</param>
        /// <param name="model">ChatClient to use as model</param>
        /// <param name="goal">(OPTIONAL) Goal for how you want to summarize</param>
        /// <param name="instructions">(OPTIONAL) extends system prompt</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Summarization text</returns>
        public async static Task<string> SummarizeAsync(this string text, ChatClient model, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<Summarization>().ToString();
            var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "summarize", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
            ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
            var systemChatMessage = GetSystemPrompt(goal ?? "summarize", instructions);
            var itemMessage = Utils.GetItemPrompt(text!);
            ChatCompletion chatCompletion = await model.CompleteChatAsync([systemChatMessage, itemMessage], options, cancellationToken: cancellationToken);
            return chatCompletion.Content.Select(completion =>
            {
#if DEBUG
                lock (model)
                {
                    Debug.WriteLine("===============================================");
                    Debug.WriteLine(systemChatMessage.Content.Single().Text);
                    Debug.WriteLine(itemMessage.Content.Single().Text);
                    Debug.WriteLine(completion.Text);
                }
#endif
                var result = JsonConvert.DeserializeObject<Summarization>(completion.Text)!;
                return result.Summary;
            }).Single()!;
        }

        /// <summary>
        /// Summarize a collection of text using OpenAI model
        /// </summary>
        /// <param name="source">source collection of text</param>
        /// <param name="model">ChatClient to use as model</param>
        /// <param name="goal">(OPTIONAL) Goal for how you want to summarize</param>
        /// <param name="instructions">(OPTIONAL) extends system prompt</param>
        /// <param name="maxParallel">(OPTIONAL) control how many concurrent tasks are executed</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>collection of summerization text</returns>
        public static IList<string> Summarize(this IEnumerable<string> source, ChatClient model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.Summarize<string>(model, goal, instructions, maxParallel, cancellationToken);
        }
        /// <summary>
        /// Summarize a collection of objects using OpenAI model
        /// </summary>
        /// <typeparam name="SourceT">source type</typeparam>
        /// <param name="source">source collection</param>
        /// <param name="model">ChatClient to use as model</param>
        /// <param name="goal">(OPTIONAL) Goal for how you want to summarize</param>
        /// <param name="instructions">(OPTIONAL) extends system prompt</param>
        /// <param name="maxParallel">(OPTIONAL) control how many concurrent tasks are executed</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>collection of summerization text</returns>
        public static IList<string> Summarize<SourceT>(this IEnumerable<SourceT> source, ChatClient model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.SelectParallelAsync(async (item, index, ct) =>
            {
                var text = item is string ? item as string : JsonConvert.SerializeObject(item).ToString();
                return await text!.SummarizeAsync(model, goal, instructions, ct);
            }, maxParallel: maxParallel ?? Environment.ProcessorCount * 2, cancellationToken: cancellationToken);
        }

        private static SystemChatMessage GetSystemPrompt(string goal, string? instructions = null)
        {
            return new SystemChatMessage($$"""
                    You are an expert at summarizing text.

                    <GOAL>
                    {{goal}}

                    <INSTRUCTIONS>
                    Given an <ITEM> summarize it using the directions in the provided <GOAL>.
                    Ensure that the summary portion is a string.{{instructions}}

                    """);
        }

    }
}

