using Helpers.DTOs.Chat;
using Helpers.HelperClasses;

namespace Services.Interfaces
{
    public interface IChatService
    {
        Task<ApiResponse<ChatMessageResponseDTO>> SendMessageAsync(ChatMessageRequestDTO request);
        Task<ApiResponse<ChatMessageResponseDTO>> SendMessageAsync(ChatMessageRequestDTO request, long? userId);
        Task<ApiResponse<ChatSessionDTO>> CreateChatSessionAsync(string? title = null);
        Task<ApiResponse<List<ChatSessionDTO>>> GetUserChatSessionsAsync(int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<ChatSessionDTO>> GetChatSessionAsync(long chatSessionId);
        Task<ApiResponse<bool>> DeactivateChatSessionAsync(long chatSessionId);
        Task<ApiResponse<UserContextDTO>> GetUserContextAsync();
        Task<ApiResponse<UserLearningAnalyticsDTO>> GetLearningAnalyticsAsync();
    }
}
