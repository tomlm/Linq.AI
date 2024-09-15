
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
        public static string Query(this ITransformer model, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItem<string>(String.Empty, $"Answer the question: {question}", instructions, cancellationToken);

        /// <summary>
        /// Ask a question about the source object and get the answer as text.
        /// </summary>
        /// <param name="model">ITransformer for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public static Task<string> QueryAsync(this ITransformer model, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<string>(string.Empty, $"Answer the question: {question}", instructions, cancellationToken);

        /// <summary>
        /// Answer a question about the source using a OpenAI model
        /// </summary>
        /// <param name="source">object to inspect</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public static ResultT Query<ResultT>(this ITransformer model, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItem<ResultT>(string.Empty, $"Answer the question: {question}", instructions, cancellationToken);

        /// <summary>
        /// Answer a question about the source using a OpenAI model
        /// </summary>
        /// <param name="model">ITransformer for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public static Task<ResultT> QueryAsync<ResultT>(this ITransformer model, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<ResultT>(String.Empty, $"Answer the question: {question}", instructions, cancellationToken);

        /// <summary>
        /// Ask a question about the source object and get the answer as text.
        /// </summary>
        /// <param name="source">object to inspect</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public static string QueryAbout(this ITransformer model, object source, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItem<string>(source, $"Answer the question about <ITEM>: {question}", instructions, cancellationToken);

        /// <summary>
        /// Ask a question about the source object and get the answer as text.
        /// </summary>
        /// <param name="source">object to inspect</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public static Task<string> QueryAboutAsync(this ITransformer model, object source, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<string>(source, $"Answer the question about <ITEM>: {question}", instructions, cancellationToken);

        /// <summary>
        /// Answer a question about the source using a OpenAI model
        /// </summary>
        /// <param name="source">object to inspect</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public static ResultT QueryAbout<ResultT>(this ITransformer model, object source, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItem<ResultT>(source, $"Answer the question about <ITEM>: {question}", instructions, cancellationToken);

        /// <summary>
        /// Answer a question about the source using a OpenAI model
        /// </summary>
        /// <param name="source">object to inspect</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public static Task<ResultT> QueryAboutAsync<ResultT>(this ITransformer model, object source, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<ResultT>(source, $"Answer the question about <ITEM>: {question}", instructions, cancellationToken);

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
        public static IList<ResultT> QueryAboutEach<ResultT>(this IEnumerable<object> source, ITransformer model, string question, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            => source.TransformItems<ResultT>(model, $"Answer the question about <ITEM>: {question}", instructions, maxParallel, cancellationToken);
    }
}

