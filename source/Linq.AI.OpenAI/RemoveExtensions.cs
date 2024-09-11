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
        public static IEnumerable<T> Remove<T>(this IEnumerable<T> source, ChatClient chatClient, string goal, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            var removeItems = source.Where(chatClient, goal, instructions, maxParallel, cancellationToken).ToList();
            return source.Where(item => !removeItems.Contains(item));
        }
    }
}

