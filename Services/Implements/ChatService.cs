using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.Chat;
using Helpers.HelperClasses;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Interfaces;
using System.Diagnostics;

namespace Services.Implements
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IAIService _aiService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IChatRepository chatRepository,
            IAIService aiService,
            ICurrentUserService currentUserService,
            ILogger<ChatService> logger)
        {
            _chatRepository = chatRepository;
            _aiService = aiService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<ChatMessageResponseDTO>> SendMessageAsync(ChatMessageRequestDTO request)
        {
            try
            {
                var userId = _currentUserService.AccountId;
                var currentTimestamp = DateTime.UtcNow; // Use consistent timestamp
                var stopwatch = Stopwatch.StartNew();

                // Create or get chat session
                ChatSession? chatSession = null;
                if (request.ChatSessionId.HasValue)
                {
                    chatSession = await _chatRepository.GetChatSessionByIdAsync(request.ChatSessionId.Value, userId);
                }

                if (chatSession == null)
                {
                    chatSession = new ChatSession
                    {
                        UserId = userId,
                        SessionTitle = GenerateSessionTitle(request.Message),
                        CreatedAt = currentTimestamp,
                        UpdatedAt = currentTimestamp,
                        IsActive = true
                    };
                    chatSession = await _chatRepository.CreateChatSessionAsync(chatSession);
                }

                // Save user message
                var userMessage = new ChatMessage
                {
                    ChatSessionId = chatSession.ChatSessionId,
                    Role = "user",
                    Content = request.Message,
                    Timestamp = currentTimestamp
                };
                await _chatRepository.CreateChatMessageAsync(userMessage);

                // Get user context if requested
                UserContextDTO? userContext = null;
                if (request.IncludeUserProgress)
                {
                    userContext = await _chatRepository.GetUserContextAsync(userId);
                    // Ensure real-time timestamp in context
                    userContext.CurrentDateTime = currentTimestamp;
                }

                // Get chat history
                var chatHistory = await GetChatHistoryAsync(chatSession.ChatSessionId, userId);

                // Generate AI response
                var aiResponseContent = await _aiService.GenerateResponseAsync(
                    request.Message,
                    userContext,
                    chatHistory);

                stopwatch.Stop();

                // Save AI response with real-time timestamp
                var aiResponseTimestamp = DateTime.UtcNow;
                var aiMessage = new ChatMessage
                {
                    ChatSessionId = chatSession.ChatSessionId,
                    Role = "assistant",
                    Content = aiResponseContent,
                    Timestamp = aiResponseTimestamp,
                    ModelUsed = "LM Studio Local",
                    ResponseTime = stopwatch.Elapsed.TotalSeconds
                };
                var savedAiMessage = await _chatRepository.CreateChatMessageAsync(aiMessage);

                // Update session timestamp
                chatSession.UpdatedAt = aiResponseTimestamp;
                await _chatRepository.UpdateChatSessionAsync(chatSession);

                var responseDto = new ChatMessageResponseDTO
                {
                    ChatMessageId = savedAiMessage.ChatMessageId,
                    ChatSessionId = chatSession.ChatSessionId,
                    Role = savedAiMessage.Role,
                    Content = savedAiMessage.Content,
                    Timestamp = savedAiMessage.Timestamp,
                    ModelUsed = savedAiMessage.ModelUsed,
                    ResponseTime = savedAiMessage.ResponseTime
                };

                return new ApiResponse<ChatMessageResponseDTO>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = responseDto,
                    Message = "Message sent successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessageAsync at {Timestamp}", DateTime.UtcNow);
                return new ApiResponse<ChatMessageResponseDTO>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Errors = [new ApiError { Code = 5001, Message = "Error processing chat message" }]
                };
            }
        }

        public async Task<ApiResponse<ChatMessageResponseDTO>> SendMessageAsync(ChatMessageRequestDTO request, long? userId)
        {
            try
            {
                // Use provided userId or try to get from current user service
                long? actualUserId = userId;
                if (!actualUserId.HasValue)
                {
                    try
                    {
                        actualUserId = _currentUserService.AccountId;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Guest user - no authentication required
                        actualUserId = null;
                    }
                }

                var currentTimestamp = DateTime.UtcNow;
                var stopwatch = Stopwatch.StartNew();

                // For guest users, create a temporary session or handle differently
                ChatSession? chatSession = null;
                if (request.ChatSessionId.HasValue && actualUserId.HasValue)
                {
                    chatSession = await _chatRepository.GetChatSessionByIdAsync(request.ChatSessionId.Value, actualUserId.Value);
                }

                // For guest users, don't create persistent sessions
                if (chatSession == null && actualUserId.HasValue)
                {
                    chatSession = new ChatSession
                    {
                        UserId = actualUserId.Value,
                        SessionTitle = GenerateSessionTitle(request.Message),
                        CreatedAt = currentTimestamp,
                        UpdatedAt = currentTimestamp,
                        IsActive = true
                    };
                    chatSession = await _chatRepository.CreateChatSessionAsync(chatSession);
                }

                // Save user message only for authenticated users
                if (chatSession != null)
                {
                    var userMessage = new ChatMessage
                    {
                        ChatSessionId = chatSession.ChatSessionId,
                        Role = "user",
                        Content = request.Message,
                        Timestamp = currentTimestamp
                    };
                    await _chatRepository.CreateChatMessageAsync(userMessage);
                }

                // Get user context if requested and user is authenticated
                UserContextDTO? userContext = null;
                if (request.IncludeUserProgress && actualUserId.HasValue)
                {
                    userContext = await _chatRepository.GetUserContextAsync(actualUserId.Value);
                    userContext.CurrentDateTime = currentTimestamp;
                }

                // Get chat history for authenticated users only
                List<ChatMessageResponseDTO> chatHistory = new List<ChatMessageResponseDTO>();
                if (chatSession != null && actualUserId.HasValue)
                {
                    chatHistory = await GetChatHistoryAsync(chatSession.ChatSessionId, actualUserId.Value);
                }

                // Generate AI response (works for both guest and authenticated users)
                var aiResponseContent = await _aiService.GenerateResponseAsync(
                    request.Message,
                    userContext,
                    chatHistory);

                stopwatch.Stop();

                // Check for context warnings in the AI response
                var isContextWarning = aiResponseContent.Contains("Context Warning");
                var isContextLimitReached = aiResponseContent.Contains("Session Full");
                var contextStatus = isContextLimitReached ? "limit_reached" : 
                                   isContextWarning ? "warning_threshold" : "normal";

                // Save AI response only for authenticated users
                ChatMessage? savedAiMessage = null;
                if (chatSession != null)
                {
                    var aiResponseTimestamp = DateTime.UtcNow;
                    var aiMessage = new ChatMessage
                    {
                        ChatSessionId = chatSession.ChatSessionId,
                        Role = "assistant",
                        Content = aiResponseContent,
                        Timestamp = aiResponseTimestamp,
                        ModelUsed = "LM Studio Local",
                        ResponseTime = stopwatch.Elapsed.TotalSeconds
                    };
                    savedAiMessage = await _chatRepository.CreateChatMessageAsync(aiMessage);
                }

                return new ApiResponse<ChatMessageResponseDTO>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = new ChatMessageResponseDTO
                    {
                        ChatMessageId = savedAiMessage?.ChatMessageId ?? 0,
                        ChatSessionId = chatSession?.ChatSessionId ?? 0,
                        Role = "assistant",
                        Content = aiResponseContent,
                        Timestamp = DateTime.UtcNow,
                        ResponseTime = stopwatch.Elapsed.TotalSeconds,
                        ModelUsed = "LM Studio Local",
                        ContextStatus = contextStatus,
                        IsContextLimitWarning = isContextWarning,
                        IsContextLimitReached = isContextLimitReached,
                        IsGuestResponse = !actualUserId.HasValue
                    },
                    Message = actualUserId.HasValue ? "Message sent successfully" : "Guest message processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessageAsync with userId parameter");
                return new ApiResponse<ChatMessageResponseDTO>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Errors = [new ApiError { Code = 5020, Message = "Error processing chat message" }]
                };
            }
        }

        public async Task<ApiResponse<ChatSessionDTO>> CreateChatSessionAsync(string? title = null)
        {
            try
            {
                var userId = _currentUserService.AccountId;
                
                var chatSession = new ChatSession
                {
                    UserId = userId,
                    SessionTitle = title ?? "New Chat Session",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var createdSession = await _chatRepository.CreateChatSessionAsync(chatSession);

                var sessionDto = new ChatSessionDTO
                {
                    ChatSessionId = createdSession.ChatSessionId,
                    SessionTitle = createdSession.SessionTitle,
                    CreatedAt = createdSession.CreatedAt,
                    UpdatedAt = createdSession.UpdatedAt,
                    IsActive = createdSession.IsActive,
                    Messages = new List<ChatMessageResponseDTO>(),
                    MessageCount = 0
                };

                return new ApiResponse<ChatSessionDTO>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = sessionDto,
                    Message = "Chat session created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat session");
                return new ApiResponse<ChatSessionDTO>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Errors = [new ApiError { Code = 5002, Message = "Error creating chat session" }]
                };
            }
        }

        public async Task<ApiResponse<List<ChatSessionDTO>>> GetUserChatSessionsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var userId = _currentUserService.AccountId;
                var sessions = await _chatRepository.GetUserChatSessionsAsync(userId, pageNumber, pageSize);

                var sessionDtos = sessions.Select(s => new ChatSessionDTO
                {
                    ChatSessionId = s.ChatSessionId,
                    SessionTitle = s.SessionTitle,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    IsActive = s.IsActive,
                    MessageCount = s.ChatMessages?.Count ?? 0
                }).ToList();

                return new ApiResponse<List<ChatSessionDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = sessionDtos,
                    Message = "Chat sessions retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user chat sessions");
                return new ApiResponse<List<ChatSessionDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Errors = [new ApiError { Code = 5003, Message = "Error retrieving chat sessions" }]
                };
            }
        }

        public async Task<ApiResponse<ChatSessionDTO>> GetChatSessionAsync(long chatSessionId)
        {
            try
            {
                var userId = _currentUserService.AccountId;
                var session = await _chatRepository.GetChatSessionByIdAsync(chatSessionId, userId);

                if (session == null)
                {
                    return new ApiResponse<ChatSessionDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Errors = [new ApiError { Code = 5004, Message = "Chat session not found" }]
                    };
                }

                var messages = await _chatRepository.GetChatMessagesAsync(chatSessionId, userId);
                var messageDtos = messages.Select(m => new ChatMessageResponseDTO
                {
                    ChatMessageId = m.ChatMessageId,
                    ChatSessionId = m.ChatSessionId,
                    Role = m.Role,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    ModelUsed = m.ModelUsed,
                    ResponseTime = m.ResponseTime
                }).ToList();

                var sessionDto = new ChatSessionDTO
                {
                    ChatSessionId = session.ChatSessionId,
                    SessionTitle = session.SessionTitle,
                    CreatedAt = session.CreatedAt,
                    UpdatedAt = session.UpdatedAt,
                    IsActive = session.IsActive,
                    Messages = messageDtos,
                    MessageCount = messageDtos.Count
                };

                return new ApiResponse<ChatSessionDTO>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = sessionDto,
                    Message = "Chat session retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat session");
                return new ApiResponse<ChatSessionDTO>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Errors = [new ApiError { Code = 5005, Message = "Error retrieving chat session" }]
                };
            }
        }

        public async Task<ApiResponse<bool>> DeactivateChatSessionAsync(long chatSessionId)
        {
            try
            {
                var userId = _currentUserService.AccountId;
                var result = await _chatRepository.DeactivateChatSessionAsync(chatSessionId, userId);

                if (!result)
                {
                    return new ApiResponse<bool>
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Data = false,
                        Errors = [new ApiError { Code = 5006, Message = "Chat session not found or cannot be deactivated" }]
                    };
                }

                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = true,
                    Message = "Chat session deactivated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating chat session");
                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Data = false,
                    Errors = [new ApiError { Code = 5007, Message = "Error deactivating chat session" }]
                };
            }
        }

        public async Task<ApiResponse<UserContextDTO>> GetUserContextAsync()
        {
            try
            {
                var userId = _currentUserService.AccountId;
                var userContext = await _chatRepository.GetUserContextAsync(userId);

                return new ApiResponse<UserContextDTO>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = userContext,
                    Message = "User context retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user context");
                return new ApiResponse<UserContextDTO>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Errors = [new ApiError { Code = 5008, Message = "Error retrieving user context" }]
                };
            }
        }

        public async Task<ApiResponse<UserLearningAnalyticsDTO>> GetLearningAnalyticsAsync()
        {
            try
            {
                var userId = _currentUserService.AccountId;
                var analytics = await _chatRepository.GetLearningAnalyticsAsync(userId);

                return new ApiResponse<UserLearningAnalyticsDTO>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = analytics,
                    Message = "Learning analytics retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learning analytics");
                return new ApiResponse<UserLearningAnalyticsDTO>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Errors = [new ApiError { Code = 5009, Message = "Error retrieving learning analytics" }]
                };
            }
        }

        private async Task<List<ChatMessageResponseDTO>> GetChatHistoryAsync(long chatSessionId, long userId)
        {
            try
            {
                var messages = await _chatRepository.GetChatMessagesAsync(chatSessionId, userId);
                return messages.Select(m => new ChatMessageResponseDTO
                {
                    ChatMessageId = m.ChatMessageId,
                    ChatSessionId = m.ChatSessionId,
                    Role = m.Role,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    ModelUsed = m.ModelUsed,
                    ResponseTime = m.ResponseTime
                }).ToList();
            }
            catch
            {
                return new List<ChatMessageResponseDTO>();
            }
        }

        private string GenerateSessionTitle(string firstMessage)
        {
            // Generate a meaningful title from the first message
            var words = firstMessage.Split(' ').Take(5);
            var title = string.Join(" ", words);
            if (title.Length > 50)
            {
                title = title.Substring(0, 47) + "...";
            }
            return title;
        }
    }
}
