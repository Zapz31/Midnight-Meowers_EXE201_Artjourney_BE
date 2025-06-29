using BusinessObjects.Models;
using Helpers.DTOs.Chat;

namespace Repositories.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatSession> CreateChatSessionAsync(ChatSession chatSession);
        Task<ChatMessage> CreateChatMessageAsync(ChatMessage chatMessage);
        Task<ChatSession?> GetChatSessionByIdAsync(long chatSessionId, long userId);
        Task<List<ChatSession>> GetUserChatSessionsAsync(long userId, int pageNumber = 1, int pageSize = 10);
        Task<List<ChatMessage>> GetChatMessagesAsync(long chatSessionId, long userId);
        Task<bool> UpdateChatSessionAsync(ChatSession chatSession);
        Task<bool> DeactivateChatSessionAsync(long chatSessionId, long userId);
        Task<UserContextDTO> GetUserContextAsync(long userId);
        Task<UserLearningAnalyticsDTO> GetLearningAnalyticsAsync(long userId);
    }
}
