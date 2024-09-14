
namespace Linq.AI
{
    public static class AnswerExtension
    {
        /// <summary>
        /// Ask a question about the source object and get the answer as text.
        /// </summary>
        /// <param name="source">object to inspect</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public static string Answer(this object source, ITransformer model, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => source.TransformItem<string>(model, $"Answer the question: {question}", instructions, cancellationToken);

        /// <summary>
        /// Answer a question about the source using a OpenAI model
        /// </summary>
        /// <param name="source">object to inspect</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public static ResultT Answer<ResultT>(this object source, ITransformer model, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => source.TransformItem<ResultT>(model, $"Answer the question: {question}", instructions, cancellationToken);

        /// <summary>
        /// Ask a question about the source object and get the answer as text.
        /// </summary>
        /// <param name="source">object to inspect</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public static Task<string> AnswerAsync(this object source, ITransformer model, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => source.TransformItemAsync<string>(model, $"Answer the question: {question}", instructions, cancellationToken);

        /// <summary>
        /// Answer a question about the source using a OpenAI model
        /// </summary>
        /// <param name="source">object to inspect</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public static Task<ResultT> AnswerAsync<ResultT>(this object source, ITransformer model, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => source.TransformItemAsync<ResultT>(model, $"Answer the question: {question}", instructions, cancellationToken);

        /// <summary>
        /// Answer a question about each item in a collection using an AI Model
        /// </summary>
        /// <typeparam name="ResultT">type of result desired</typeparam>
        /// <param name="source">collection of items</param>
        /// <param name="model">model to use</param>
        /// <param name="question">question to ask about each item</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="maxParallel">(OPTIONAL) max parallelization to use</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>an answer in the form of ResultT</returns>
        public static IList<ResultT> Answer<ResultT>(this IEnumerable<object> source, ITransformer model, string question, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            => source.TransformItems<ResultT>(model, $"Answer the question: {question}", instructions, maxParallel, cancellationToken);
    }
}

