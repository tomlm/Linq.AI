using Iciclecreek.Async;
using Newtonsoft.Json;
using OpenAI.Chat;

namespace Linq.AI.OpenAI
{

    public static class MiscExtensions
    {
        /// <summary>
        /// Generate an item of shape T based on "goal"
        /// </summary>
        /// <typeparam name="ResultT">type of items</typeparam>
        /// <param name="source">collection of items</param>
        /// <param name="constraint">The constraint to match to remove an item</param>
        /// <param name="instructions">(OPTIONAL) additional instructions</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel operations</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>collection of items which didn't match the goal</returns>
        public static ResultT Generate<ResultT>(this ChatClient model, string goal, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            => model.TransformItem<ResultT>(String.Empty, goal, instructions, cancellationToken);

        /// <summary>
        /// Generate an item of shape T based on "goal"
        /// </summary>
        /// <typeparam name="T">type of items</typeparam>
        /// <param name="source">collection of items</param>
        /// <param name="constraint">The constraint to match to remove an item</param>
        /// <param name="instructions">(OPTIONAL) additional instructions</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel operations</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>collection of items which didn't match the goal</returns>
        public static Task<ResultT> GenerateAsync<ResultT>(this ChatClient model, string goal, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<ResultT>(String.Empty, goal, instructions, cancellationToken);
    }
}

