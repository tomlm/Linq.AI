using Microsoft.Extensions.AI;

namespace Linq.AI.Microsoft
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
        public required ChatOptions Options { get; set; }

        /// <summary>
        /// Last completion
        /// </summary>
        public ChatResponse  Result { get; set; } = null!;

        /// <summary>
        /// Last completion
        /// </summary>
        public ChatFinishReason? Completion => Result.FinishReason;

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
