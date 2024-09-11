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

    internal class SummarizationItem<ItemT> : Summarization
    {
        public ItemT? Item { get; set; }

        public int? Index { get; set; }
    }

    public static class SummarizeExtension
    {
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
        public static IEnumerable<string> SummarizeAI<SourceT>(this IEnumerable<SourceT> source, ChatClient chatClient, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {

            var schema = StructuredSchemaGenerator.FromType<Summarization>().ToString();

            var count = source.Count();

            return source.SelectParallelAsync(async (item, index) =>
            {
                var itemResult = item;
                var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "summarize", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
                ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
                var systemChatMessage = GetSystemPrompt(goal ?? "summarize", instructions);
                var itemMessage = GetItemPrompt(itemResult!);
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
                    var result = JsonConvert.DeserializeObject<SummarizationItem<SourceT>>(completion.Text)!;
                    return result.Summary;
                }).Single()!;
            }, maxParallel: maxParallel ?? int.MaxValue);
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

        private static UserChatMessage GetItemPrompt(object item)
        {
            if (!(item is string))
            {
                item = JToken.FromObject(item).ToString();
            }
            return new UserChatMessage($"""
            <ITEM>
            {item}
            """);
        }
    }
}

