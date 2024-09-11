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
        /// Summarize the text
        /// </summary>
        /// <param name="text"></param>
        /// <param name="chatClient"></param>
        /// <param name="goal"></param>
        /// <param name="instructions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async static Task<string> SummarizeAsync(this string text, ChatClient model, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<Summarization>().ToString();
            var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "summarize", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
            ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
            var systemChatMessage = GetSystemPrompt(goal ?? "summarize", instructions);
            var itemMessage = GetItemPrompt(text!);
            ChatCompletion chatCompletion = await model.CompleteChatAsync([systemChatMessage, itemMessage], options);
            return chatCompletion.Content.Select(completion =>
            {
                if (Debugger.IsAttached)
                {
                    lock (model)
                    {
                        Debug.WriteLine("===============================================");
                        Debug.WriteLine(systemChatMessage.Content.Single().Text);
                        Debug.WriteLine(itemMessage.Content.Single().Text);
                        Debug.WriteLine(completion.Text);
                    }
                }
                var result = JsonConvert.DeserializeObject<Summarization>(completion.Text)!;
                return result.Summary;
            }).Single()!;
        }

        /// <summary>
        /// Filter each item in the list using a LLM query
        /// </summary>
        /// <typeparam name="SourceT"></typeparam>
        /// <param name="source"></param>
        /// <param name="goal">The goal</param>
        /// <param name="categories">the categories</param>
        /// <param name="instructions">additional instructions</param>
        /// <param name="maxParallel">parallezation</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns></returns>
        public static IList<string> Summarize<SourceT>(this IEnumerable<SourceT> source, ChatClient model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.SelectParallelAsync(async (item, index) =>
            {
                var text = item is string ? item as string : JsonConvert.SerializeObject(item).ToString();
                return await text!.SummarizeAsync(model, goal, instructions, cancellationToken);
            }, maxParallel: maxParallel ?? Environment.ProcessorCount * 2);
        }

        private static SystemChatMessage GetSystemPrompt(string goal, string? instructions = null)
        {
            return new SystemChatMessage($$"""
                    You are an expert at summarizing text.

                    <GOAL>
                    {{goal}}

                    <INSTRUCTIONS>
                    Given an <ITEM> summarize it using the directions in the provided <GOAL>.
                    Return your summary as a JSON <SUMMARIZATION> object.
                    Ensure that the summary portion is a string.{{instructions}}

                    <SUMMARIZATION>
                    {"explanation": "<explanation supporting your summarization>", "summary": "<item summary as text>"}
                    """);
        }

        private static UserChatMessage GetItemPrompt(string item)
        {
            return new UserChatMessage($"""
            <ITEM>
            {item}
            """);
        }
    }
}

