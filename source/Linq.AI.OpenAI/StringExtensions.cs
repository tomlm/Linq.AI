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
        /// <summary>
        /// Transform text into collection of objects into objects using OpenAI model
        /// </summary>
        /// <typeparam name="T">the type of result</typeparam>
        /// <param name="text">source text</param>
        /// <param name="model">ChatClient model</param>
        /// <param name="goal">(OPTIONAL) The goal describing the transformation desired</param>
        /// <param name="instructions">(OPTIONAL) extend system instructions</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>collection of objects found in text</returns>
        public static async Task<IList<T>> SelectAsync<T>(this string text, ChatClient model, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<Extraction<T>>().ToString();

            var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "extract", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
            ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
            var systemChatMessage = GetSystemPrompt(goal ?? "extract objects from the text", instructions);
            var itemMessage = Utils.GetItemPrompt(text);
            ChatCompletion chatCompletion = await model.CompleteChatAsync([systemChatMessage, itemMessage], options, cancellationToken: cancellationToken);
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