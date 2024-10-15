using OpenAI.Chat;
using System.ClientModel;

namespace Linq.AI.OpenAI
{
    public class CompletionContext
    {
        /// <summary>
        /// Transformation item
        /// </summary>
        public object? Item { get; set; }

        /// <summary>
        /// Options used for the completion
        /// </summary>
        public required ChatCompletionOptions Options { get; set; }

        /// <summary>
        /// Last completion
        /// </summary>
        public ClientResult<ChatCompletion> Result { get; set; } = null!;

        /// <summary>
        /// Last completion
        /// </summary>
        public ChatCompletion Completion => Result.Value;

        /// <summary>
        /// Conversation history
        /// </summary>
        public List<ChatMessage> Messages { get; } = new List<ChatMessage>();

        /// <summary>
        /// Results of calling functions.
        /// </summary>
        public Dictionary<string, object?> ToolResults { get; } = new Dictionary<string, object?>();
    }

}
