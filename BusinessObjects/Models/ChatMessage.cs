using BusinessObjects.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models
{
    public class ChatMessage
    {
        [Key]
        public long ChatMessageId { get; set; }
        
        [ForeignKey("ChatSession")]
        public long ChatSessionId { get; set; }
        public virtual ChatSession? ChatSession { get; set; }
        
        public string Role { get; set; } = string.Empty; // "user" or "assistant"
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // Optional: Store AI model response metadata
        public string? ModelUsed { get; set; }
        public int? TokensUsed { get; set; }
        public double? ResponseTime { get; set; }
    }
}
