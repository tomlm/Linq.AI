
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
        public static string Summarize(this object source, ITransformer model, string? goal, string? instructions = null, CancellationToken cancellationToken = default)
            => source.TransformItem<string>(model, goal ?? "summarize", instructions, cancellationToken);

        /// <summary>
        /// Summarize text using OpenAI model
        /// </summary>
        /// <param name="source">source to summarize</param>
        /// <param name="model">ITransformer to use as model</param>
        /// <param name="goal">(OPTIONAL) summarization desired</param>
        /// <param name="instructions">(OPTIONAL) additional instructions on how to summarize</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>Summarization text</returns>
        public static Task<string> SummarizeAsync(this object source, ITransformer model, string? goal, string? instructions = null, CancellationToken cancellationToken = default)
            => source.TransformItemAsync<string>(model, goal ?? "summarize", instructions, cancellationToken);

        /// <summary>
        /// Summarize each element in a collection using OpenAI model
        /// </summary>
        /// <param name="source">source collection</param>
        /// <param name="model">ITransformer to use as model</param>
        /// <param name="goal">(OPTIONAL) summarization desired</param>
        /// <param name="instructions">(OPTIONAL) additional instructions on how to summarize</param>
        /// <param name="maxParallel">(OPTIONAL) control how many concurrent tasks are executed</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>collection of summerization text</returns>
        public static IList<string> Summarize(this IEnumerable<object> source, ITransformer model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            => source.TransformItems<string>(model, goal ?? "summarize", instructions, maxParallel, cancellationToken);
    }
}

