
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
        public static EnumT Classify<EnumT>(this object source, ITransformer model, string? instructions = null, CancellationToken cancellationToken = default)
            where EnumT : struct, Enum
            => source.TransformItem<EnumT>(model, "classify", instructions, cancellationToken);

        /// <summary>
        /// Classify source by list of categories using AI model
        /// </summary>
        /// <param name="source">source to process</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="categories">collection of categories</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to classify.</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>string from categories which best matches.</returns>
        public static string Classify(this object source, ITransformer model, IList<string> categories, string? instructions = null, CancellationToken cancellationToken = default)
            => source.TransformItem<string>(model, $"classify into categories: [{String.Join(",", categories)}]", instructions, cancellationToken);

        /// <summary>
        /// Classify source by enum using AI model
        /// </summary>
        /// <typeparam name="EnumT">enumeration to use as classification categories</typeparam>
        /// <param name="source">source to process</param>
        /// <param name="model">chat client to use for the model</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to classify.</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>enumeration for category which best matches</returns>
        public static Task<EnumT> ClassifyAsync<EnumT>(this object source, ITransformer model, string? instructions = null, CancellationToken cancellationToken = default)
            where EnumT : struct, Enum
            => source.TransformItemAsync<EnumT>(model, "classify", instructions, cancellationToken);

        /// <summary>
        /// Classify source by list of categories using AI model
        /// </summary>
        /// <param name="source">source to process</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="categories">collection of categories</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to classify.</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>string from categories which best matches.</returns>
        public static Task<string> ClassifyAsync(this object source, ITransformer model, IList<string> categories, string? instructions = null, CancellationToken cancellationToken = default)
            => source.TransformItemAsync<string>(model, $"classify into categories: [{String.Join(",", categories)}]", instructions, cancellationToken);

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
                var category = await item!.ClassifyAsync<EnumT>(model, instructions, ct);
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
                var category = await item!.ClassifyAsync(model, categories, instructions, ct);
                return new ClassifiedItem<ItemT, string>()
                {
                    Item = item,
                    Category = category
                };
            }, maxParallel: maxParallel ?? 2 * Environment.ProcessorCount, cancellationToken: cancellationToken);
    }
}

