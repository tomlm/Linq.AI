
using Iciclecreek.Async;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OpenAI.Chat;
using System.Diagnostics;

namespace Linq.AI.OpenAI
{

    internal class WhereItem
    {
        public string? Explanation { get; set; } = null;

        public bool Matches { get; set; }
    }

    internal class WhereItem<T> : WhereItem
    {
        public T? Item { get; set; }
    }

    public static class WhereExtension
    {
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
        public static IEnumerable<T> WhereAI<T>(this IEnumerable<T> source, ChatClient chatClient, string goal, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            // TODO get this from someplace...
            var schema = StructuredSchemaGenerator.FromType<WhereItem>().ToString();

            var count = source.Count();

            return source.SelectParallelAsync(async (item, index) =>
                {
                    var itemResult = item;
                    var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "Where", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
                    ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
                    var systemChatMessage = GetSystemPrompt(goal, instructions);
                    var itemMessage = GetItemPrompt(itemResult!, index, count);
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
                        var result = JsonConvert.DeserializeObject<WhereItem<T>>(completion.Text)!;
                        result.Item = itemResult;
                        return result;
                    }).Single()!;
                }, maxParallel: maxParallel ?? int.MaxValue)
                .Where(result => result.Matches)
                .Select(result => result.Item!);
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

        private static UserChatMessage GetItemPrompt(object item, int index, int length)
        {
            if (!(item is string))
            {
                item = JToken.FromObject(item).ToString();
            }
            return new UserChatMessage($"""
            <INDEX>
            {index} of {length}

            <ITEM>
            {item}
            
            """);
        }
    }
}

