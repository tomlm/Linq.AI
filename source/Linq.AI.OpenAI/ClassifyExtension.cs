
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
        /// Classify text into enum using AI model
        /// </summary>
        /// <typeparam name="EnumT">enumeration to use as classification categories</typeparam>
        /// <param name="text">text to process</param>
        /// <param name="model">chat client to use for the model</param>
        /// <param name="goal">(optional) override goal. Default is to classify item</param>
        /// <param name="instructions">(optional) extend instructions.</param>
        /// <param name="cancellationToken">(optional) cancellation token</param>
        /// <returns>enumeration for category which best matches</returns>
        public async static Task<EnumT> ClassifyAsync<EnumT>(this string text, ChatClient model, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
            where EnumT : struct, Enum
        {
            var categories = Enum.GetValues<EnumT>().Select(val => val.ToString()).ToList();
            var category = await text.ClassifyAsync(model, categories, goal, instructions, cancellationToken);
            return Enum.Parse<EnumT>(category);
        }


        /// <summary>
        /// Classify text from list of categories using AI model
        /// </summary>
        /// <param name="text">text to classifiy</param>
        /// <param name="model">ChatClient for model</param>
        /// <param name="categories">collection of categories</param>
        /// <param name="goal">(OPTIONAL) override goal. Default is to classify item</param>
        /// <param name="instructions">(OPTIONAL) extend instructions.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>string from categories which best matches.</returns>
        public async static Task<string> ClassifyAsync(this string text, ChatClient model, IList<string> categories, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<ClassifiedItem>().ToString();
            var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "classify", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
            ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
            var systemChatMessage = GetSystemPrompt(goal ?? "classify", categories, instructions);
            var itemMessage = Utils.GetItemPrompt(text!);
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
                var result = JsonConvert.DeserializeObject<ClassifiedItem>(completion.Text)!;
                return result.Category!;
            }).Single()!;
        }

        /// <summary>
        /// Classify collection of text using Enum and AI model
        /// </summary>
        /// <typeparam name="EnumT">enumeration to use for categories</typeparam>
        /// <param name="source">collection of text to classifiy</param>
        /// <param name="model">ChatClient for model</param>
        /// <param name="goal">(OPTIONAL) override goal. Default is to classify item</param>
        /// <param name="instructions">(OPTIONAL) extend instructions.</param>
        /// <param name="maxParallel">(OPTIONAL) max paralell queries to make</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>list of classifications</returns>
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
        /// Classify collection of text using collection of categories and AI model
        /// </summary>
        /// <param name="source">collection of text to classifiy</param>
        /// <param name="model">ChatClient for model</param>
        /// <param name="categories">categories to use</param>
        /// <param name="goal">(OPTIONAL) override goal. Default is to classify item</param>
        /// <param name="instructions">(OPTIONAL) extend instructions.</param>
        /// <param name="maxParallel">(OPTIONAL) max paralell queries to make</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>list of classifications</returns>
        public static IList<ClassifiedItem<string, string>> Classify(this IEnumerable<string> source, ChatClient model, IList<string> categories, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.Classify<string>(model, categories, goal, instructions, maxParallel, cancellationToken);
        }

        /// <summary>
        /// Classify collection of objects using colllection of categories and AI model
        /// </summary>
        /// <param name="source">collection of text to classifiy</param>
        /// <param name="model">ChatClient for model</param>
        /// <param name="categories">categories to use</param>
        /// <param name="goal">(OPTIONAL) override goal. Default is to classify item</param>
        /// <param name="instructions">(OPTIONAL) extend instructions.</param>
        /// <param name="maxParallel">(OPTIONAL) max paralell queries to make</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>list of classifications</returns>
        public static IList<ClassifiedItem<SourceT, string>> Classify<SourceT>(this IEnumerable<SourceT> source, ChatClient model, IList<string> categories, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            var count = source.Count();

            return source.SelectParallelAsync(async (item, index, ct) =>
            {
                var text = (item is string) ? (item as string) : JsonConvert.SerializeObject(item).ToString()!;
                var category = await text!.ClassifyAsync(model, categories, goal, instructions, ct);
                return new ClassifiedItem<SourceT, string>()
                {
                    Item = item,
                    Category = category
                };
            }, maxParallel: maxParallel ?? Environment.ProcessorCount * 2, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Classify collection of objects using enum for categoriesand AI model
        /// </summary>
        /// <typeparam name="SourceT">item type to categories</typeparam>
        /// <typeparam name="EnumT">Enumeration to use for categories</typeparam>
        /// <param name="source">collection of text to classifiy</param>
        /// <param name="model">ChatClient for model</param>
        /// <param name="goal">(OPTIONAL) override goal. Default is to classify item</param>
        /// <param name="instructions">(OPTIONAL) extend instructions.</param>
        /// <param name="maxParallel">(OPTIONAL) max paralell queries to make</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>list of classifications</returns>
        public static IList<ClassifiedItem<SourceT, EnumT>> Classify<SourceT, EnumT>(this IEnumerable<SourceT> source, ChatClient model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            where EnumT : struct, Enum
        {
            var count = source.Count();

            return source.SelectParallelAsync(async (item, index, ct) =>
            {
                var text = (item is string) ? (item as string) : JsonConvert.SerializeObject(item).ToString()!;
                var category = await text!.ClassifyAsync<EnumT>(model, goal, instructions, ct);
                return new ClassifiedItem<SourceT, EnumT>()
                {
                    Item = item,
                    Category = category
                };
            }, maxParallel: maxParallel ?? Environment.ProcessorCount * 2, cancellationToken: cancellationToken);
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

