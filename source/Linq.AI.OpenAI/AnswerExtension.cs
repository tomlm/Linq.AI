using Iciclecreek.Async;
using Newtonsoft.Json;
using OpenAI.Chat;
using System.ComponentModel;
using System.Diagnostics;

namespace Linq.AI.OpenAI
{
    internal class AnswerItem
    {
        [Description("Explain your reasoning")]
        public string? Explanation { get; set; }

        [Description("The answer to the user question")]
        public string? Answer { get; set; }
    }

    public static class AnswerExtension
    {
        /// <summary>
        /// Answer a question about the text
        /// </summary>
        /// <param name="text"></param>
        /// <param name="chatClient"></param>
        /// <param name="question"></param>
        /// <param name="instructions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async static Task<string> AnswerAsync(this string text, ChatClient model, string question, string? instructions = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<AnswerItem>().ToString();
            var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "answer", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
            ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
            var systemChatMessage = GetSystemPrompt(text!, instructions);
            var itemMessage = Utils.GetItemPrompt(question);
            ChatCompletion chatCompletion = await model.CompleteChatAsync([systemChatMessage, itemMessage], options);
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
                return JsonConvert.DeserializeObject<AnswerItem>(completion.Text)!.Answer;
            }).Single()!;
        }


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
        public static IList<string> Answer<SourceT>(this IEnumerable<SourceT> source, ChatClient model, string question, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.SelectParallelAsync(async (item, index) =>
            {
                var text = (item is string) ? item as string : JsonConvert.SerializeObject(item).ToString();
                return await text!.AnswerAsync(model, question, instructions, cancellationToken);
            }, maxParallel: maxParallel ?? Environment.ProcessorCount * 2);
        }

        private static SystemChatMessage GetSystemPrompt(string context, string? instructions = null)
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
                    Do not directly mention that you're using the context in your answer.
                    {{instructions ?? String.Empty}}

                    """);
        }

    }
}

