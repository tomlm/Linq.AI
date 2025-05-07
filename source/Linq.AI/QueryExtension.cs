namespace Linq.AI
{
    public static class AnswerExtension
    {
        /// <summary>
        /// Ask a question about the source object and get the answer as text.
        /// </summary>
        /// <param name="model">ITransformer for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public static ValueTask<string> QueryAsync(this ITransformer model, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<string>(string.Empty, $"Answer the question: {question}", instructions, cancellationToken);

        /// <summary>
        /// Answer a question about the source using a OpenAI model
        /// </summary>
        /// <param name="model">ITransformer for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public static ValueTask<ResultT> QueryAsync<ResultT>(this ITransformer model, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<ResultT>(String.Empty, $"Answer the question: {question}", instructions, cancellationToken);

        /// <summary>
        /// Ask a question about the source object and get the answer as text.
        /// </summary>
        /// <param name="item">object to inspect</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public static ValueTask<string> QueryAboutAsync(this ITransformer model, object item, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<string>(item, $"Answer the question about <ITEM>: {question}", instructions, cancellationToken);

        /// <summary>
        /// Answer a question about the source using a OpenAI model
        /// </summary>
        /// <param name="item">object to inspect</param>
        /// <param name="model">ITransformer for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public static ValueTask<ResultT> QueryAbout<ResultT>(this ITransformer model, object item, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<ResultT>(item, $"Answer the question about <ITEM>: {question}", instructions, cancellationToken);

        /// <summary>
        /// Asnwer a question about each element in a collection.
        /// </summary>
        /// <typeparam name="ResultT"></typeparam>
        /// <param name="source"></param>
        /// <param name="model"></param>
        /// <param name="question"></param>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public static IAsyncEnumerable<ResultT> QueryAboutAsync<ResultT>(this IEnumerable<object> source, ITransformer model, string question, string? instructions = null)
            => source
                .ToAsyncEnumerable()
                .QueryAboutAsync<ResultT>(model, question, instructions);

        /// <summary>
        /// Answer a question about each item in a collection using an AI Model
        /// </summary>
        /// <typeparam name="ResultT">type of result desired</typeparam>
        /// <param name="source">collection of items</param>
        /// <param name="model">model to use</param>
        /// <param name="question">question to ask about each item</param>
        /// <param name="instructions">(OPTIONAL) instructions for how to answer</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token to cancel the operation.</param>
        /// <returns>an answer in the form of ResultT</returns>
        public static IAsyncEnumerable<ResultT> QueryAboutAsync<ResultT>(this IAsyncEnumerable<object> source, ITransformer model, string question, string? instructions = null)
            => source.SelectAwaitWithCancellation((item, index, ct) => model.TransformItemAsync<ResultT>(item, $"Answer the question about <ITEM>: {question}", instructions, ct));
    }
}

