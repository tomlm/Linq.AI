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
        /// Answer a question about the text using a OpenAI model
        /// </summary>
        /// <param name="text">text to inspect</param>
        /// <param name="model">ChatClient for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">system instructions to help control the answer</param>
        /// <param name="cancellationToken">cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public async static Task<string> AnswerAsync(this string text, ChatClient model, string question, string? instructions = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<AnswerItem>().ToString();
            var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "answer", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
            ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
            var systemChatMessage = GetSystemPrompt(text!, instructions);
            var itemMessage = Utils.GetItemPrompt(question);
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
                return JsonConvert.DeserializeObject<AnswerItem>(completion.Text)!.Answer;
            }).Single()!;
        }


        /// <summary>
        /// Answer a question about each item in a collection using an AI Model
        /// </summary>
        /// <typeparam name="SourceT">type of the item</typeparam>
        /// <param name="source">collection of items</param>
        /// <param name="model">model to use</param>
        /// <param name="question">question to ask about each item</param>
        /// <param name="instructions">system instructions to extend system prompt</param>
        /// <param name="maxParallel">max parallelization to use</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns></returns>
        public static IList<string> Answer<SourceT>(this IEnumerable<SourceT> source, ChatClient model, string question, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.SelectParallelAsync(async (item, index, ct) =>
            {
                var text = (item is string) ? item as string : JsonConvert.SerializeObject(item).ToString();
                return await text!.AnswerAsync(model, question, instructions, cancellationToken);
            }, maxParallel: maxParallel ?? Environment.ProcessorCount * 2, cancellationToken: cancellationToken);
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

