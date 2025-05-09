
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Linq.AI
{

    public static class SelectExtension
    {
        /// <summary>
        /// Transform text into collection of objects into objects using OpenAI model
        /// </summary>
        /// <typeparam name="T">the type of result</typeparam>
        /// <param name="source">source</param>
        /// <param name="model">ITransformer model</param>
        /// <param name="goal">(OPTIONAL) The goal describing the transformation desired</param>
        /// <param name="instructions">(OPTIONAL) additional instructions on how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>collection of objects found in text</returns>
        public async static Task<IList<T>> ExtractAsync<T>(this ITransformer model, object source, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
            => (await model.TransformItemAsync<T[]>(source, goal, instructions, cancellationToken)).ToList();

        /// <summary>
        /// Transform collection of ojects into ResulT using LLM model.
        /// </summary>
        /// <typeparam name="ResultT"></typeparam>
        /// <param name="source"></param>
        /// <param name="model"></param>
        /// <param name="goal"></param>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public static IAsyncEnumerable<ResultT> SelectAsync<ResultT>(this IEnumerable<object> source, ITransformer model, string? goal = null, string? instructions = null)
            => source
                .ToAsyncEnumerable()
                .SelectAsync<ResultT>(model, goal, instructions);

        /// <summary>
        /// Transform collection of text into text using OpenAI model
        /// </summary>
        /// <param name="source">collection of text</param>
        /// <param name="model">ITransformer model</param>
        /// <param name="goal">(OPTIONAL) The goal describing the transformation desired</param>
        /// <param name="instructions">(OPTIONAL) additional instructions on how to transform</param>
        /// <returns>collection of transformed text</returns>
        public static IAsyncEnumerable<ResultT> SelectAsync<ResultT>(this IAsyncEnumerable<object> source, ITransformer model, string? goal = null, string? instructions = null)
            => source.SelectAwaitWithCancellation((item, index, ct) => model.TransformItemAsync<ResultT>(item, goal, instructions, ct));
    }
}

