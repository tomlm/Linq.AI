using Iciclecreek.Async;

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
        public static bool Matches(this object source, ITransformer model, string constraint, string? instructions = null, CancellationToken cancellationToken = default)
            => source.TransformItem<bool>(model, $"Does the <ITEM> match this contraint => {constraint}?", instructions, cancellationToken);

        /// <summary>
        /// Determine if text matches goal
        /// </summary>
        /// <param name="source">source to inspect</param>
        /// <param name="model">ITransformer to use for model</param>
        /// <param name="constraint">constraint to use for matching</param>
        /// <param name="instructions">(OPTIONAL) optional extension of system prompt</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>true/false</returns>
        public static Task<bool> MatchesAsync(this object source, ITransformer model, string constraint, string? instructions = null, CancellationToken cancellationToken = default)
            => source.TransformItemAsync<bool>(model, $"Does the <ITEM> match this contraint => {constraint}?", instructions, cancellationToken);

        /// <summary>
        /// Determine if items matches goal
        /// </summary>
        /// <typeparam name="T">type of enumerable</typeparam>
        /// <param name="source">source collection of objects</param>
        /// <param name="model">ITransformer to use for model</param>
        /// <param name="constraint">constraint to use for matching on each item</param>
        /// <param name="instructions">(OPTIONAL) optional extension of system prompt</param>
        /// <param name="maxParallel">(OPTIONAL) controls number of concurrent tasks executed</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>collection of objects that match the goal</returns>
        public static IList<T> Where<T>(this IEnumerable<T> source, ITransformer model, string constraint, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            var count = source.Count();
            return source.WhereParallelAsync((item, index, ct) => 
                item!.MatchesAsync(model, constraint, Utils.GetItemIndexClause(index, count, instructions), cancellationToken), 
                maxParallel: maxParallel ?? 2 * Environment.ProcessorCount, cancellationToken: cancellationToken);
        }
    }
}

