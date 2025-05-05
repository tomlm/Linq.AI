using System.Runtime.CompilerServices;

namespace Linq.AI
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
        /// <param name="source">source to inspect</param>
        /// <param name="model">ITransformer to use for model</param>
        /// <param name="constraint">constraint to use for matching</param>
        /// <param name="instructions">(OPTIONAL) optional extension of system prompt</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>true/false</returns>
        public static ValueTask<bool> MatchesAsync(this ITransformer model, object source, string constraint, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<bool>(source, $"Does the <ITEM> match this contraint => {constraint}?", instructions, cancellationToken);

        /// <summary>
        /// Enumerate each item in the collection and use LLM model to determine if it matches the goal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="model"></param>
        /// <param name="constraint"></param>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public static IAsyncEnumerable<T> WhereAsync<T>(this IEnumerable<T> source, ITransformer model, string constraint, string? instructions = null)
            => source
                .ToAsyncEnumerable()
                .WhereAsync(model, constraint, instructions);

        /// <summary>
        /// Determine if items matches goal
        /// </summary>
        /// <typeparam name="T">type of enumerable</typeparam>
        /// <param name="source">source collection of objects</param>
        /// <param name="model">ITransformer to use for model</param>
        /// <param name="constraint">constraint to use for matching on each item</param>
        /// <param name="instructions">(OPTIONAL) optional extension of system prompt</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>collection of objects that match the goal</returns>
        public static IAsyncEnumerable<T> WhereAsync<T>(this IAsyncEnumerable<T> source, ITransformer model, string constraint, string? instructions = null)
            => source.WhereAwaitWithCancellation(async (item, index, ct) => await model.MatchesAsync(item!, constraint, Utils.GetItemIndexClause(index, instructions)));
    }
}

