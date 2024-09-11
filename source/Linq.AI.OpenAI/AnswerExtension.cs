using Iciclecreek.Async;
using Newtonsoft.Json;
using OpenAI.Chat;
using System.Diagnostics;

namespace Linq.AI.OpenAI
{
    internal class AnswerItem
    {
        public string? Explanation { get; set; }

        public string? Answer { get; set; }
    }

    public static class AnswerExtension
    {
        /// <summary>
        /// Answer a question about the item.
        /// </summary>
        /// <typeparam name="SourceT">type of the item</typeparam>
        /// <param name="source">collection of items</param>
        /// <param name="chatClient"></param>
        /// <param name="question">question to ask</param>
        /// <param name="instructions"></param>
        /// <param name="maxParallel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static IEnumerable<string> Answer<SourceT>(this IEnumerable<SourceT> source, ChatClient chatClient, string question, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<AnswerItem>().ToString();

            var count = source.Count();

            return source.SelectParallelAsync(async (item, index) =>
            {
                var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "answer", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
                ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
                var systemChatMessage = GetSystemPrompt(item!, instructions);
                var itemMessage = new UserChatMessage(question);
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
                        }
                        Debug.WriteLine(completion.Text);
                    }
                    return JsonConvert.DeserializeObject<AnswerItem>(completion.Text)!.Answer;
                }).Single()!;
            }, maxParallel: maxParallel ?? int.MaxValue);
        }

        private static SystemChatMessage GetSystemPrompt(object context, string? instructions = null)
        {
            if (!(context is string))
            {
                context = JsonConvert.SerializeObject(context, Formatting.Indented);
            }
            return new SystemChatMessage($$"""
                    <CONTEXT>
                    {{context}}

                    <INSTRUCTIONS>
                    Base your answer only on the information provided in the above <CONTEXT>.
                    Return your answer using the JSON <OUTPUT> below. 
                    Do not directly mention that you're using the context in your answer.{{instructions}}

                    <OUTPUT>
                    {"explanation": "<explain your reasoning>", "answer": "<the answer>"}`;
                    """);
        }

    }
}

