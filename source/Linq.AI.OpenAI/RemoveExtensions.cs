using Iciclecreek.Async;
using Newtonsoft.Json;
using OpenAI.Chat;

namespace Linq.AI.OpenAI
{

    public static class RemoveAllExtension
    {
        /// <summary>
        /// Remove all items that match the goal using OpenAI model
        /// </summary>
        /// <typeparam name="T">type of items</typeparam>
        /// <param name="source">collection of items</param>
        /// <param name="goal">The goal which will be applied to each item</param>
        /// <param name="instructions">(OPTIONAL) extend system instructions</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel tasks running queries</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>collection of items which didn't match the goal</returns>
        public static IList<T> Remove<T>(this IEnumerable<T> source, ChatClient model, string goal, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.WhereParallelAsync(async (item, index, ct) =>
            {
                var text = (item is string) ? item as string : JsonConvert.SerializeObject(item).ToString();
                return !(await text!.MatchesAsync(model, goal, instructions, ct));
            }, maxParallel: maxParallel ?? Environment.ProcessorCount * 2, cancellationToken: cancellationToken);
        }
    }
}

