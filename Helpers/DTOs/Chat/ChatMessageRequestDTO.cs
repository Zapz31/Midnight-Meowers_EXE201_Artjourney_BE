using System.ComponentModel.DataAnnotations;

namespace Helpers.DTOs.Chat
{
    public class ChatMessageRequestDTO
    {
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public long? ChatSessionId { get; set; }
        
        // Optional: Include user context for better AI responses
        public bool IncludeUserProgress { get; set; } = true;
        public bool IncludeCurrentCourse { get; set; } = true;
    }
}
