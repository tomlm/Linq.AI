
using Iciclecreek.Async;

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
        public static EnumT Classify<EnumT>(this ITransformer model, object source, string? instructions = null, CancellationToken cancellationToken = default)
            where EnumT : struct, Enum
            => model.TransformItem<EnumT>(source, "classify", instructions, cancellationToken);

        /// <summary>
        /// Classify source by list of categories using AI model
        /// </summary>
        /// <param name="source">source to process</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="categories">collection of categories</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to classify.</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>string from categories which best matches.</returns>
        public static string Classify(this ITransformer model, object source, IList<string> categories, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItem<string>(source, $"classify into categories: [{String.Join(",", categories)}]", instructions, cancellationToken);

        /// <summary>
        /// Classify source by enum using AI model
        /// </summary>
        /// <typeparam name="EnumT">enumeration to use as classification categories</typeparam>
        /// <param name="source">source to process</param>
        /// <param name="model">chat client to use for the model</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to classify.</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>enumeration for category which best matches</returns>
        public static Task<EnumT> ClassifyAsync<EnumT>(this ITransformer model, object source, string? instructions = null, CancellationToken cancellationToken = default)
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
        public static Task<string> ClassifyAsync(this ITransformer model, object source, IList<string> categories, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<string>(source, $"classify into categories: [{String.Join(",", categories)}]", instructions, cancellationToken);

        public static IList<ClassifiedItem<string, EnumT>> Classify<EnumT>(this IEnumerable<string> source, ITransformer model, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            where EnumT : struct, Enum
            => source.Classify<string, EnumT>(model, instructions, maxParallel, cancellationToken);

        /// <summary>
        /// Classify collection of items using Enum and AI model
        /// </summary>
        /// <typeparam name="EnumT">enumeration to use for categories</typeparam>
        /// <param name="source">collection of text to classifiy</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to classify.</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel operations</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>list of classifications</returns>
        public static IList<ClassifiedItem<ItemT, EnumT>> Classify<ItemT, EnumT>(this IEnumerable<ItemT> source, ITransformer model, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            where EnumT : struct, Enum
            => source.SelectParallelAsync(async (item, index, ct) =>
            {
                var category = await model.ClassifyAsync<EnumT>(item!, instructions, ct);
                return new ClassifiedItem<ItemT, EnumT>()
                {
                    Item = item,
                    Category = category
                };
            }, maxParallel: maxParallel ?? 2 * Environment.ProcessorCount, cancellationToken: cancellationToken);

        /// <summary>
        /// Classify collection of text using collection of categories and AI model
        /// </summary>
        /// <param name="source">collection of text to classifiy</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="categories">categories to use</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to classify.</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel operations</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>list of classifications</returns>
        public static IList<ClassifiedItem<ItemT, string>> Classify<ItemT>(this IEnumerable<ItemT> source, ITransformer model, IList<string> categories, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            where ItemT : class
            => source.SelectParallelAsync(async (item, index, ct) =>
            {
                var category = await model.ClassifyAsync(item, categories, instructions, ct);
                return new ClassifiedItem<ItemT, string>()
                {
                    Item = item,
                    Category = category
                };
            }, maxParallel: maxParallel ?? 2 * Environment.ProcessorCount, cancellationToken: cancellationToken);
    }
}

