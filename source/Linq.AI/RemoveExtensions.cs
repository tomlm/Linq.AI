using Iciclecreek.Async;
using Newtonsoft.Json;

namespace Linq.AI
{

    public static class RemoveAllExtension
    {
        /// <summary>
        /// Remove all items that match the goal using OpenAI model
        /// </summary>
        /// <typeparam name="T">type of items</typeparam>
        /// <param name="source">collection of items</param>
        /// <param name="model">ITransformer model</param>
        /// <param name="constraint">The constraint to match to remove an item</param>
        /// <param name="instructions">(OPTIONAL) additional instructions</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel operations</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>collection of items which didn't match the goal</returns>
        public static IList<T> Remove<T>(this IEnumerable<T> source, ITransformer model, string constraint, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            var count = source.Count();

            return source.WhereParallelAsync(async (item, index, ct) =>
            {
                var text = (item is string) ? item as string : JsonConvert.SerializeObject(item).ToString();
                var matches = await item!.MatchesAsync(model, constraint, Utils.GetItemIndexClause(index, count, instructions), ct);
                return !matches;
            }, maxParallel: maxParallel ?? Environment.ProcessorCount * 2, cancellationToken: cancellationToken);
        }
    }
}

