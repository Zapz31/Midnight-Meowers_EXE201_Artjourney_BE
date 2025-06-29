namespace Helpers.DTOs.Chat
{
    public class ChatMessageResponseDTO
    {
        public long ChatMessageId { get; set; }
        public long ChatSessionId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? ModelUsed { get; set; }
        public double? ResponseTime { get; set; }
        
        // Context management
        public string? ContextStatus { get; set; }
        public bool IsContextLimitWarning { get; set; }
        public bool IsContextLimitReached { get; set; }
        
        // Guest user support
        public bool IsGuestResponse { get; set; }
    }
}
