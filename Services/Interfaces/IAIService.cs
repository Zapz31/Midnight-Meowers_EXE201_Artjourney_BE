using Helpers.DTOs.Chat;

namespace Services.Interfaces
{
    public interface IAIService
    {
        Task<string> GenerateResponseAsync(string userMessage, UserContextDTO? userContext = null, List<ChatMessageResponseDTO>? chatHistory = null);
        IAsyncEnumerable<string> GenerateStreamingResponseAsync(string userMessage, UserContextDTO? userContext = null, List<ChatMessageResponseDTO>? chatHistory = null);
        Task<bool> IsServiceAvailableAsync();
        string BuildSystemPrompt(UserContextDTO? userContext = null);
        string BuildMinimalSystemPrompt(UserContextDTO? userContext = null);
    }
}
