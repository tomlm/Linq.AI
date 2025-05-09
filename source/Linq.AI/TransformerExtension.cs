using System.Threading;
using System.Threading.Tasks;

namespace Linq.AI
{
    public static class TransformerExtension
    {
        /// <summary>
        /// Generate text using OpenAI model Async
        /// </summary>
        /// <param name="model">ITransformer to use as model</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>transformed text</returns>
        public static ValueTask<string> GenerateAsync(this ITransformer model, string goal, string? instructions = null, CancellationToken cancellationToken = default)
            => model.GenerateAsync<string>(goal, instructions, cancellationToken);

        /// <summary>
        /// TransformItemAsync as string default Async
        /// </summary>
        /// <param name="model">ITransformer to use as model</param>
        /// <param name="item">item to transform</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>transformed text</returns>
        public static ValueTask<string> TransformItem(this ITransformer model, object item, string goal, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<string>(item, goal, instructions, cancellationToken);
    }
}

