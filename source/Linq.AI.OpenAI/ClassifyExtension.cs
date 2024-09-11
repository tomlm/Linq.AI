
using Iciclecreek.Async;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.Chat;
using System.Diagnostics;

namespace Linq.AI.OpenAI
{

    public class ClassifiedItem
    {
        public string? Explanation { get; set; } = null;

        public string? Category { get; set; } = null;
    }

    public class ClassifiedItem<ItemT, CategoryT>
    {
        public string? Explanation { get; set; } = null;

        public CategoryT Category { get; set; } = default!;

        public ItemT Item { get; set; } = default!;
    }

    public static class ClassifyExtension
    {
        /// <summary>
        /// Use AI to Clasifiy each item in a string collection into an enumeration 
        /// </summary>
        /// <typeparam name="EnumT">enumeration to use</typeparam>
        /// <param name="source"></param>
        /// <param name="goal"></param>
        /// <param name="instructions"></param>
        /// <param name="maxParallel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static IEnumerable<ClassifiedItem<string, EnumT>> Classify<EnumT>(this IEnumerable<string> source, ChatClient chatClient, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            where EnumT : struct, Enum
        {
            var categories = Enum.GetValues<EnumT>().Select(val => val.ToString()).ToList();
            return source.Classify(chatClient, categories, goal, instructions, maxParallel, cancellationToken)
                .Select(result => new ClassifiedItem<string, EnumT>()
                {
                    Item = result.Item,
                    Category = Enum.Parse<EnumT>(result.Category),
                    Explanation = result.Explanation,
                });
        }

        /// <summary>
        /// Use AI to Clasifiy each item in a string collection into an enumeration 
        /// </summary>
        /// <typeparam name="EnumT">enumeration to use</typeparam>
        /// <param name="source"></param>
        /// <param name="goal"></param>
        /// <param name="instructions"></param>
        /// <param name="maxParallel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static IEnumerable<ClassifiedItem<string, string>> Classify(this IEnumerable<string> source, ChatClient chatClient, IList<string> categories, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.Classify<string>(chatClient, categories, goal, instructions, maxParallel, cancellationToken);
        }

        /// <summary>
        /// Classify each item in list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="categories">the categories</param>
        /// <param name="goal">The goal</param>
        /// <param name="instructions">additional instructions</param>
        /// <param name="maxParallel">parallezation</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns></returns>
        public static IEnumerable<ClassifiedItem<T, string>> Classify<T>(this IEnumerable<T> source, ChatClient chatClient, IList<string> categories, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            // TODO get this from someplace...
            var schema = StructuredSchemaGenerator.FromType<ClassifiedItem>().ToString();

            var count = source.Count();

            return source.SelectParallelAsync(async (item, index) =>
            {
                var itemResult = item;
                var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "classify", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
                ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
                var systemChatMessage = GetSystemPrompt(goal ?? "classify", categories, instructions);
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
                    var result = JsonConvert.DeserializeObject<ClassifiedItem>(completion.Text)!;
                    return new ClassifiedItem<T, string>()
                    {
                        Category = result.Category!,
                        Item = itemResult,
                        Explanation = result.Explanation
                    };
                }).Single()!;
            }, maxParallel: maxParallel ?? int.MaxValue);
        }

        private static SystemChatMessage GetSystemPrompt(string goal, IList<string> categories, string? instructions = null)
        {
            var cat = $"* {String.Join("* ", categories.Select(c => $"{c.Trim()}\n"))}";
            return new SystemChatMessage($$"""
                    You are an expert at classifying items in a list.
                    <CATEGORIES>
                    {{cat}}

                    <GOAL>
                    {{goal.Trim()}}

                    <INSTRUCTIONS>
                    Given an <ITEM> classify the item using the above <CATEGORIES> based upon the provided <GOAL>.
                    Return your classification as a JSON <CLASSIFICATION> object.{{instructions ?? String.Empty}}

                    <CLASSIFICATION>
                    {"explanation": "<explanation supporting your classification>", "category": "<category assigned>"}
                    
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

