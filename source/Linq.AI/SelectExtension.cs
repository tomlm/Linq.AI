﻿
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
        public static IList<T> Select<T>(this ITransformer model, object source, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItem<T[]>(source, goal, instructions, cancellationToken).ToList();

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
        public async static Task<IList<T>> SelectAsync<T>(this ITransformer model, object source, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
            => (await model.TransformItemAsync<T[]>(source, goal, instructions, cancellationToken)).ToList();

        /// <summary>
        /// Transform collection of text into text using OpenAI model
        /// </summary>
        /// <param name="source">collection of text</param>
        /// <param name="model">ITransformer model</param>
        /// <param name="goal">(OPTIONAL) The goal describing the transformation desired</param>
        /// <param name="instructions">(OPTIONAL) additional instructions on how to transform</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel tasks running queries</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>collection of transformed text</returns>
        public static IList<ResultT> Select<ResultT>(this IEnumerable<object> source, ITransformer model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        => source.TransformItems<ResultT>(model, goal, instructions, maxParallel, cancellationToken);
    }
}

