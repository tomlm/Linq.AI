
using Iciclecreek.Async;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OpenAI.Chat;
using System;
using System.Data;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace Linq.AI.OpenAI
{

    internal class WhereItem
    {
        public string? Explanation { get; set; } = null;

        public bool Matches { get; set; }
    }

    public static class WhereExtension
    {
        /// <summary>
        /// Returns true/false based on weather it matches the constraint
        /// </summary>
        /// <param name="text"></param>
        /// <param name="chatClient"></param>
        /// <param name="constraint"></param>
        /// <param name="instructions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async static Task<bool> MatchesAsync(this string text, ChatClient model, string constraint, string? instructions = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<WhereItem>().ToString();
            var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "Where", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
            ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
            var systemChatMessage = GetSystemPrompt(constraint, instructions);
            var itemMessage = Utils.GetItemPrompt(text, 0, 1);
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
                var result = JsonConvert.DeserializeObject<WhereItem>(completion.Text)!;
                return result.Matches;
            }).Single()!;
        }


        /// <summary>
        /// Classify each item in list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="goal">The goal</param>
        /// <param name="categories">the categories</param>
        /// <param name="instructions">additional instructions</param>
        /// <param name="maxParallel">parallezation</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns></returns>
        public static IList<T> Where<T>(this IEnumerable<T> source, ChatClient model, string goal, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            var count = source.Count();

            return source.WhereParallelAsync(async (item, index) =>
            {
                var schema = StructuredSchemaGenerator.FromType<WhereItem>().ToString();
                var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "Where", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
                ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
                var systemChatMessage = GetSystemPrompt(goal, instructions);
                var itemMessage = Utils.GetItemPrompt(item, index, count);
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
                    var result = JsonConvert.DeserializeObject<WhereItem>(completion.Text)!;
                    return result.Matches;
                }).Single()!;
            }, maxParallel: maxParallel ?? Environment.ProcessorCount * 2);
        }

        private static SystemChatMessage GetSystemPrompt(string goal, string? instructions = null)
        {
            return new SystemChatMessage($$"""
                You are an expert at classifying items in a list.

                <GOAL>
                {{goal}} 

                <INSTRUCTIONS>
                Given an <ITEM> determine if it matches the provided <GOAL>.
                The item index starts at 0.
                {{instructions}}

                """);
        }

    }
}

