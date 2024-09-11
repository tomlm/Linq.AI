using Iciclecreek.Async;
using Newtonsoft.Json;
using OpenAI.Chat;

namespace Linq.AI.OpenAI
{

    public static class RemoveAllExtension
    {
        /// <summary>
        /// Remove all items that match the goal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="goal">The goal</param>
        /// <param name="categories">the categories</param>
        /// <param name="instructions">additional instructions</param>
        /// <param name="maxParallel">parallezation</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns></returns>
        public static IList<T> Remove<T>(this IEnumerable<T> source, ChatClient model, string goal, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            return source.WhereParallelAsync(async (item, index) =>
            {
                var text = (item is string) ? item as string : JsonConvert.SerializeObject(item).ToString();
                return !(await text!.MatchesAsync(model, goal, instructions, cancellationToken));
            }, maxParallel: maxParallel ?? Environment.ProcessorCount * 2);
        }
    }
}

