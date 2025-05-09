using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Linq.AI
{
    /// <summary>
    /// AI Transformer interface
    /// </summary>
    public interface ITransformer
    {
        /// <summary>
        /// Generate an item of ResultT using goal and instructions
        /// </summary>
        /// <typeparam name="ResultT">result type</typeparam>
        /// <param name="goal">The goal for what you want to generte</param>
        /// <param name="instructions">(OPTIONAL) additional instructions</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>collection of items which didn't match the goal</returns>
        ValueTask<ResultT> GenerateAsync<ResultT>(string goal, string? instructions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Transform item using shape ResulT Async using goal and instructions
        /// </summary>
        /// <typeparam name="ResultT">result type</typeparam>
        /// <param name="item">item to Transform</param>
        /// <param name="model">ChatClient to use as model</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>transformed result</returns>
        ValueTask<ResultT> TransformItemAsync<ResultT>(object item, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Transform items into ShapeT using goal and instructions
        /// </summary>
        /// <typeparam name="ResultT">result type</typeparam>
        /// <param name="source">source collection</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>transformed results</returns>
        IAsyncEnumerable<ResultT> TransformItemsAsync<ResultT>(IEnumerable<object> source, string? goal = null, string? instructions = null);

        /// <summary>
        /// Transform items into ShapeT using goal and instructions
        /// </summary>
        /// <typeparam name="ResultT">result type</typeparam>
        /// <param name="source">source collection</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>transformed results</returns>
        IAsyncEnumerable<ResultT> TransformItemsAsync<ResultT>(IAsyncEnumerable<object> source, string? goal = null, string? instructions = null);
    }
}
