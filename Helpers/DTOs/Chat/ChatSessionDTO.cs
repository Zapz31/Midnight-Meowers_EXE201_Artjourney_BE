namespace Helpers.DTOs.Chat
{
    public class ChatSessionDTO
    {
        public long ChatSessionId { get; set; }
        public string SessionTitle { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<ChatMessageResponseDTO> Messages { get; set; } = new List<ChatMessageResponseDTO>();
        public int MessageCount { get; set; }
    }
}
