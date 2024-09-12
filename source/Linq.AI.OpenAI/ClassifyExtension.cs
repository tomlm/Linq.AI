
using Iciclecreek.Async;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.Chat;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Linq.AI.OpenAI
{

    public class ClassifiedItem
    {
        [Description("Explain your reasoning")]
        public string? Explanation { get; set; } = null;

        [Description("The category which the item best belongs in")]
        public string? Category { get; set; } = null;
    }

    public class ClassifiedItem<ItemT, CategoryT>
    {
        public CategoryT Category { get; set; } = default!;

        public ItemT Item { get; set; } = default!;
    }

    public static class ClassifyExtension
    {
        /// <summary>
        /// Classify text into enum
        /// </summary>
        /// <typeparam name="EnumT"></typeparam>
        /// <param name="text"></param>
        /// <param name="chatClient"></param>
        /// <param name="goal"></param>
        /// <param name="instructions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async static Task<EnumT> ClassifyAsync<EnumT>(this string text, ChatClient model, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
            where EnumT : struct, Enum
        {
            var categories = Enum.GetValues<EnumT>().Select(val => val.ToString()).ToList();
            var category = await text.ClassifyAsync(model, categories, goal, instructions, cancellationToken);
            return Enum.Parse<EnumT>(category);
        }


        /// <summary>
        /// Classify text from list of categories
        /// </summary>
        /// <param name="text"></param>
        /// <param name="chatClient"></param>
        /// <param name="categories"></param>
        /// <param name="goal"></param>
        /// <param name="instructions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async static Task<string> ClassifyAsync(this string text, ChatClient model, IList<string> categories, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<ClassifiedItem>().ToString();
            var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "classify", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
            ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
            var systemChatMessage = GetSystemPrompt(goal ?? "classify", categories, instructions);
            var itemMessage = Utils.GetItemPrompt(text!);
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
                var result = JsonConvert.DeserializeObject<ClassifiedItem>(completion.Text)!;
                return result.Category!;
            }).Single()!;
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
        public static IList<ClassifiedItem<string, EnumT>> Classify<EnumT>(this IEnumerable<string> source, ChatClient model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            where EnumT : struct, Enum
        {
            var categories = Enum.GetValues<EnumT>().Select(val => val.ToString()).ToList();
            return source
                .Classify(model, categories, goal, instructions, maxParallel, cancellationToken)
                .Select(result => new ClassifiedItem<string, EnumT>()
                {
                    Item = result.Item,
                    Category = Enum.Parse<EnumT>(result.Category),
                }).ToList();
        }

        /// <summary>
        /// Use AI to Clasifiy each item in a string collection into an categories
        /// </summary>
        /// <typeparam name="EnumT">enumeration to use</typeparam>
        /// <param name="source"></param>
        /// <param name="goal"></param>
        /// <param name="instructions"></param>
        /// <param name="maxParallel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static IList<ClassifiedItem<string, string>> Classify(this IEnumerable<string> source, ChatClient model, IList<string> categories, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.Classify<string>(model, categories, goal, instructions, maxParallel, cancellationToken);
        }

        /// <summary>
        /// Classify each item in list using categories
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="categories">the categories</param>
        /// <param name="goal">The goal</param>
        /// <param name="instructions">additional instructions</param>
        /// <param name="maxParallel">parallezation</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns></returns>
        public static IList<ClassifiedItem<T, string>> Classify<T>(this IEnumerable<T> source, ChatClient model, IList<string> categories, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            var count = source.Count();

            return source.SelectParallelAsync(async (item, index) =>
            {
                var text = (item is string) ? (item as string) : JsonConvert.SerializeObject(item).ToString()!;
                var category = await text!.ClassifyAsync(model, categories, goal, instructions, cancellationToken);
                return new ClassifiedItem<T, string>()
                {
                    Item = item,
                    Category = category
                };
            }, maxParallel: maxParallel ?? Environment.ProcessorCount * 2);
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
                    Return your classification as a JSON <CLASSIFICATION> object.
                    {{instructions ?? String.Empty}}

                    """);
        }

    }
}

