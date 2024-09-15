namespace Linq.AI
{
    public static class TransformExtension
    {
        /// <summary>
        /// Transform text using OpenAI model
        /// </summary>
        /// <param name="item">item to Transform</param>
        /// <param name="model">ITransformer to use as model</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>transformed text</returns>
        public static ResultT TransformItem<ResultT>(this ITransformer model, object item, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<ResultT>(item, goal, instructions, cancellationToken).Result;

        /// <summary>
        /// Transform text using OpenAI model
        /// </summary>
        /// <param name="item">item to Transform</param>
        /// <param name="model">ITransformer to use as model</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>transformed text</returns>
        public static Task<ResultT> TransformItemAsync<ResultT>(this ITransformer model, object item, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<ResultT>(item, goal, instructions, cancellationToken);

        /// <summary>
        /// Transform items using pool of background tasks and AI model
        /// </summary>
        /// <typeparam name="ResultT">result type</typeparam>
        /// <param name="source">source collection</param>
        /// <param name="model">ai model to use</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel operations</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>transformed results</returns>
        public static IList<ResultT> TransformItems<ResultT>(this IEnumerable<object> source, ITransformer model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            => model.TransformItems<ResultT>(source, goal, instructions, maxParallel, cancellationToken);


    }
}

