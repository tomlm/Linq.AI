

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Linq.AI
{

    public class ClassifiedItem<ItemT, CategoryT>
    {
        public CategoryT Category { get; set; } = default!;

        public ItemT Item { get; set; } = default!;
    }

    public static class ClassifyExtension
    {
        /// <summary>
        /// Classify source by enum using AI model
        /// </summary>
        /// <typeparam name="EnumT">enumeration to use as classification categories</typeparam>
        /// <param name="source">source to process</param>
        /// <param name="model">chat client to use for the model</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to classify.</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>enumeration for category which best matches</returns>
        public static ValueTask<EnumT> ClassifyAsync<EnumT>(this ITransformer model, object source, string? instructions = null, CancellationToken cancellationToken = default)
            where EnumT : struct, Enum
            => model.TransformItemAsync<EnumT>(source, "classify", instructions, cancellationToken);

        /// <summary>
        /// Classify source by list of categories using AI model
        /// </summary>
        /// <param name="source">source to process</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="categories">collection of categories</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to classify.</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>string from categories which best matches.</returns>
        public static ValueTask<string> ClassifyAsync(this ITransformer model, object source, IList<string> categories, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<string>(source, $"classify into categories: [{String.Join(",", categories)}]", instructions, cancellationToken);

        /// <summary>
        /// Classify each string in the enumeration using async Classify Model.
        /// </summary>
        /// <typeparam name="EnumT"></typeparam>
        /// <param name="source"></param>
        /// <param name="model"></param>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public static IAsyncEnumerable<ClassifiedItem<string, string>> ClassifyAsync(this IEnumerable<string> source, ITransformer model, IList<string> categories, string? instructions = null)
            => source
                .ToAsyncEnumerable()
                .ClassifyAsync<string>(model, categories, instructions);

        /// <summary>
        /// Classify each string in the enumeration using async Classify Model.
        /// </summary>
        /// <typeparam name="EnumT"></typeparam>
        /// <param name="source"></param>
        /// <param name="model"></param>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public static IAsyncEnumerable<ClassifiedItem<string, EnumT>> ClassifyAsync<EnumT>(this IEnumerable<string> source, ITransformer model, string? instructions = null)
           where EnumT : struct, Enum
            => source
                .ToAsyncEnumerable()
                .ClassifyAsync<EnumT>(model, instructions);

        /// <summary>
        /// Classify each item in the enumeration using async Classify Model.
        /// </summary>
        /// <typeparam name="EnumT"></typeparam>
        /// <param name="source"></param>
        /// <param name="model"></param>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public static IAsyncEnumerable<ClassifiedItem<ItemT, string>> ClassifyAsync<ItemT>(this IEnumerable<ItemT> source, ITransformer model, IList<string> categories, string? instructions = null)
            where ItemT : class
            => source
                .ToAsyncEnumerable()
                .ClassifyAsync<ItemT>(model, categories, instructions);

        /// <summary>
        /// Classify each item in the enumeration using async Classify Model.
        /// </summary>
        /// <typeparam name="EnumT"></typeparam>
        /// <param name="source"></param>
        /// <param name="model"></param>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public static IAsyncEnumerable<ClassifiedItem<ItemT, EnumT>> ClassifyAsync<ItemT, EnumT>(this IEnumerable<ItemT> source, ITransformer model, string? instructions = null)
            where EnumT : struct, Enum
            => source
                .ToAsyncEnumerable()
                .ClassifyAsync<ItemT, EnumT>(model, instructions);

        /// <summary>
        /// Classify collection of items using Enum and AI model
        /// </summary>
        /// <typeparam name="EnumT">enumeration to use for categories</typeparam>
        /// <param name="source">collection of text to classifiy</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to classify.</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>list of classifications</returns>
        public static IAsyncEnumerable<ClassifiedItem<ItemT, EnumT>> ClassifyAsync<ItemT, EnumT>(this IAsyncEnumerable<ItemT> source, ITransformer model, string? instructions = null)
            where EnumT : struct, Enum
            => source.SelectAwaitWithCancellation(async (item, index, ct) =>
            {
                var category = await model.ClassifyAsync<EnumT>(item!, instructions, ct);
                return new ClassifiedItem<ItemT, EnumT>()
                {
                    Item = item,
                    Category = category
                };
            });

        /// <summary>
        /// Classify collection of text using collection of categories and AI model
        /// </summary>
        /// <param name="source">collection of text to classifiy</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="categories">categories to use</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to classify.</param>
        /// <returns>classifications</returns>
        public static IAsyncEnumerable<ClassifiedItem<ItemT, string>> ClassifyAsync<ItemT>(this IAsyncEnumerable<ItemT> source, ITransformer model, IList<string> categories, string? instructions = null)
            where ItemT : class
            => source.SelectAwaitWithCancellation(async (item, index, ct) =>
            {
                var category = await model.ClassifyAsync(item, categories, instructions, ct);
                return new ClassifiedItem<ItemT, string>()
                {
                    Item = item,
                    Category = category
                };
            });

        /// <summary>
        /// Classify each string in the enumeration using async Classify Model.
        /// </summary>
        /// <typeparam name="EnumT"></typeparam>
        /// <param name="source"></param>
        /// <param name="model"></param>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public static IAsyncEnumerable<ClassifiedItem<string, EnumT>> ClassifyAsync<EnumT>(this IAsyncEnumerable<string> source, ITransformer model, string? instructions = null)
            where EnumT : struct, Enum
            => source.ClassifyAsync<string, EnumT>(model, instructions);

    }
}

