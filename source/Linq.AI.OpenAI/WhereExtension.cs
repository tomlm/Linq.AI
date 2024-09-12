
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
        /// Determine if text matches goal
        /// </summary>
        /// <param name="text">text to inspect</param>
        /// <param name="model">ChatClient to use for model</param>
        /// <param name="goal">goal for matching</param>
        /// <param name="instructions">(OPTIONAL) optional extension of system prompt</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>true/false</returns>
        public async static Task<bool> MatchesAsync(this string text, ChatClient model, string goal, string? instructions = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<WhereItem>().ToString();
            var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "Where", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
            ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
            var systemChatMessage = GetSystemPrompt(goal, instructions);
            var itemMessage = Utils.GetItemPrompt(text, 0, 1);
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
                var result = JsonConvert.DeserializeObject<WhereItem>(completion.Text)!;
                return result.Matches;
            }).Single()!;
        }


        /// <summary>
        /// Determine if items matches goal
        /// </summary>
        /// <typeparam name="T">type of enumerable</typeparam>
        /// <param name="source">source collection of objects</param>
        /// <param name="model">ChatClient to use for model</param>
        /// <param name="goal">goal for matching</param>
        /// <param name="instructions">(OPTIONAL) optional extension of system prompt</param>
        /// <param name="maxParallel">(OPTIONAL) controls number of concurrent tasks executed</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>collection of objects that match the goal</returns>
        public static IList<T> Where<T>(this IEnumerable<T> source, ChatClient model, string goal, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            var count = source.Count();

            return source.WhereParallelAsync(async (item, index, ct) =>
            {
                var schema = StructuredSchemaGenerator.FromType<WhereItem>().ToString();
                var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "Where", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
                ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
                var systemChatMessage = GetSystemPrompt(goal, instructions);
                var itemMessage = Utils.GetItemPrompt(item!, index, count);
                ChatCompletion chatCompletion = await model.CompleteChatAsync([systemChatMessage, itemMessage], options, cancellationToken);
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
            }, maxParallel: maxParallel ?? Environment.ProcessorCount * 2, cancellationToken: cancellationToken);
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

