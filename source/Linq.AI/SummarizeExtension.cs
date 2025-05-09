
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Linq.AI
{

    internal class Summarization
    {
        public string? Explanation { get; set; }

        public string? Summary { get; set; }
    }

    public static class SummarizeExtension
    {
        /// <summary>
        /// Summarize text using OpenAI model
        /// </summary>
        /// <param name="source">source to summarize</param>
        /// <param name="model">ITransformer to use as model</param>
        /// <param name="goal">(OPTIONAL) summarization desired</param>
        /// <param name="instructions">(OPTIONAL) additional instructions on how to summarize</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>Summarization text</returns>
        public static ValueTask<string> SummarizeAsync(this ITransformer model, object source, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<string>(source, goal ?? "summarize", instructions, cancellationToken);

        /// <summary>
        /// Summarize each element in a collection using model and goal.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="model"></param>
        /// <param name="goal"></param>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public static IAsyncEnumerable<string> SummarizeAsync(this IEnumerable<object> source, ITransformer model, string? goal = null, string? instructions = null)
            => source
                .ToAsyncEnumerable()
                .SummarizeAsync(model, goal, instructions);

        /// <summary>
        /// Summarize each element in a collection using OpenAI model
        /// </summary>
        /// <param name="source">source collection</param>
        /// <param name="model">ITransformer to use as model</param>
        /// <param name="goal">(OPTIONAL) summarization desired</param>
        /// <param name="instructions">(OPTIONAL) additional instructions on how to summarize</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>collection of summerization text</returns>
        public static IAsyncEnumerable<string> SummarizeAsync(this IAsyncEnumerable<object> source, ITransformer model, string? goal = null, string? instructions = null)
            => source.SelectAwaitWithCancellation<object, string>((item, index, ct) => model.SummarizeAsync(item, goal ?? "summarize", instructions, ct));
    }
}

