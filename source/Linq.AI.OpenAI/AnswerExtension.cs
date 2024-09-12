using OpenAI.Chat;

namespace Linq.AI.OpenAI
{
    public static class AnswerExtension
    {
        /// <summary>
        /// Answer a question about the text using a OpenAI model
        /// </summary>
        /// <param name="text">text to inspect</param>
        /// <param name="model">ChatClient for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">system instructions to help control the answer</param>
        /// <param name="cancellationToken">cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public static Task<ResultT> AnswerAsync<ResultT>(this object source, ChatClient model, string question, string? instructions = null, CancellationToken cancellationToken = default)
            => source.TransformItemAsync<ResultT>(model, question, instructions, cancellationToken);


        /// <summary>
        /// Answer a question about each item in a collection using an AI Model
        /// </summary>
        /// <typeparam name="SourceT">type of the item</typeparam>
        /// <param name="source">collection of items</param>
        /// <param name="model">model to use</param>
        /// <param name="question">question to ask about each item</param>
        /// <param name="instructions">system instructions to extend system prompt</param>
        /// <param name="maxParallel">max parallelization to use</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns></returns>
        public static IList<ResultT> Answer<ResultT>(this IEnumerable<object> source, ChatClient model, string question, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            => source.TransformItems<ResultT>(model, question, instructions, maxParallel, cancellationToken);
    }
}

