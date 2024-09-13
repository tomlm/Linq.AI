using OpenAI.Chat;

namespace Linq.AI.OpenAI
{

    public static class SelectExtension
    {
        /// <summary>
        /// Transform text into collection of objects into objects using OpenAI model
        /// </summary>
        /// <typeparam name="T">the type of result</typeparam>
        /// <param name="source">source</param>
        /// <param name="model">ChatClient model</param>
        /// <param name="goal">(OPTIONAL) The goal describing the transformation desired</param>
        /// <param name="instructions">(OPTIONAL) extend system instructions</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>collection of objects found in text</returns>
        public static IList<T> Select<T>(this object source, ChatClient model, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
            => source.TransformItem<T[]>(model, goal, instructions, cancellationToken).ToList();

        /// <summary>
        /// Transform text into collection of objects into objects using OpenAI model
        /// </summary>
        /// <typeparam name="T">the type of result</typeparam>
        /// <param name="source">source</param>
        /// <param name="model">ChatClient model</param>
        /// <param name="goal">(OPTIONAL) The goal describing the transformation desired</param>
        /// <param name="instructions">(OPTIONAL) extend system instructions</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>collection of objects found in text</returns>
        public async static Task<IList<T>> SelectAsync<T>(this object source, ChatClient model, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
            => (await source.TransformItemAsync<T[]>(model, goal, instructions, cancellationToken)).ToList();

        /// <summary>
        /// Transform collection of text into text using OpenAI model
        /// </summary>
        /// <param name="source">collection of text</param>
        /// <param name="model">ChatClient model</param>
        /// <param name="goal">(OPTIONAL) The goal describing the transformation desired</param>
        /// <param name="instructions">(OPTIONAL) extend system instructions</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel tasks running queries</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>collection of transformed text</returns>
        public static IList<ResultT> Select<ResultT>(this IEnumerable<object> source, ChatClient model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        => source.TransformItems<ResultT>(model, goal, instructions, maxParallel, cancellationToken);
    }
}

