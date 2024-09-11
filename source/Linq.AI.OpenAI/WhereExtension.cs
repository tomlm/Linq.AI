
using Iciclecreek.Async;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OpenAI.Chat;
using System;
using System.Diagnostics;

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
        public async static Task<bool> Matches(this string text, ChatClient model, string constraint, string? instructions = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<WhereItem>().ToString();
            var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "Where", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
            ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
            var systemChatMessage = GetSystemPrompt(constraint, instructions);
            var itemMessage = GetItemPrompt(text);
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
        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, ChatClient model, string goal, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.WhereParallelAsync(async (item, index) =>
            {
                var text = (item is string) ? item as string : JsonConvert.SerializeObject(item).ToString();
                return await text!.Matches(model, goal, instructions, cancellationToken);
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
                Return your classification as a JSON <CLASSIFICATION> object.{{instructions}}

                <CLASSIFICATION>
                {"explanation": "<explanation supporting your classification>", "matches": <true or false>}
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

