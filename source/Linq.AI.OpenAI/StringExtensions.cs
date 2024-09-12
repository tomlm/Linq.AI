using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.Chat;
using System.Diagnostics;

namespace Linq.AI.OpenAI
{
    internal class Extraction<T>
    {
        public ExtractedItem<T>[]? List { get; set; }
    }

    internal class ExtractedItem<T>
    {
        public string? Explanation { get; set; }

        public T? Item { get; set; }
    }

    public static class StringExtensions
    {
        /// Extract all items that match the goal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="goal">The goal</param>
        /// <param name="categories">the categories</param>
        /// <param name="instructions">additional instructions</param>
        /// <param name="maxParallel">parallezation</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns></returns>
        public static async Task<IList<T>> SelectAsync<T>(this string text, ChatClient model, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<Extraction<T>>().ToString();

            var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "extract", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
            ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
            var systemChatMessage = GetSystemPrompt(goal ?? "extract objects from the text", instructions);
            var itemMessage = Utils.GetItemPrompt(text);
            ChatCompletion chatCompletion = await model.CompleteChatAsync([systemChatMessage, itemMessage], options);
#if DEBUG
            Debug.WriteLine("===============================================");
            Debug.WriteLine(systemChatMessage.Content.Single().Text);
            Debug.WriteLine(itemMessage.Content.Single().Text);
            Debug.WriteLine(chatCompletion.Content.Single().Text);
#endif
            var extraction = JsonConvert.DeserializeObject<Extraction<T>>(chatCompletion.Content.Single().Text)!;
            return extraction.List!.Select(item => item.Item!).ToList();
        }

        private static SystemChatMessage GetSystemPrompt(string goal, string? instructions = null)
        {
            return new SystemChatMessage($$"""
                    You are an expert at extracting a list from text.

                    <GOAL>
                    {{goal}} 

                    <INSTRUCTIONS>
                    Extract from text a JSON <OUTPUT> object using the <GOAL> to define each item of the list.
                    {{instructions}}

                    """);
        }
    }
}