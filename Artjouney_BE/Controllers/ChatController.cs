using Helpers.DTOs.Chat;
using Helpers.HelperClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using BusinessObjects.Enums;

namespace Artjouney_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IAIService _aiService;
        private readonly ILogger<ChatController> _logger;
        private readonly ICurrentUserService _currentUserService;

        public ChatController(IChatService chatService, IAIService aiService, ILogger<ChatController> logger, ICurrentUserService currentUserService)
        {
            _chatService = chatService;
            _aiService = aiService;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Send a message to the AI chatbot
        /// </summary>
        /// <param name="request">Chat message request</param>
        /// <returns>AI response</returns>
        [HttpPost("message")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ChatMessageResponseDTO>>> SendMessage([FromBody] ChatMessageRequestDTO request)
        {
            try
            {
                _logger.LogInformation("Received chat message from user at {Timestamp}", DateTime.UtcNow);
                
                // Check if AI service is available
                if (!await _aiService.IsServiceAvailableAsync())
                {
                    return BadRequest(new ApiResponse<ChatMessageResponseDTO>
                    {
                        Status = BusinessObjects.Enums.ResponseStatus.Error,
                        Code = 503,
                        Errors = [new ApiError { Code = 5009, Message = "AI service is currently unavailable. Please make sure LM Studio is running." }]
                    });
                }

                var response = await _chatService.SendMessageAsync(request);

                if (response.Status == BusinessObjects.Enums.ResponseStatus.Success)
                {
                    _logger.LogInformation("Chat message processed successfully at {Timestamp}", DateTime.UtcNow);
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message at {Timestamp}", DateTime.UtcNow);
                return StatusCode(500, new ApiResponse<ChatMessageResponseDTO>
                {
                    Status = BusinessObjects.Enums.ResponseStatus.Error,
                    Code = 500,
                    Errors = [new ApiError { Code = 5010, Message = "Internal server error while processing chat message" }]
                });
            }
        }

        /// <summary>
        /// Create a new chat session
        /// </summary>
        /// <param name="title">Optional session title</param>
        /// <returns>Created chat session</returns>
        [HttpPost("session")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ChatSessionDTO>>> CreateChatSession([FromQuery] string? title = null)
        {
            try
            {
                _logger.LogInformation("Creating new chat session at {Timestamp}", DateTime.UtcNow);
                var response = await _chatService.CreateChatSessionAsync(title);

                if (response.Status == BusinessObjects.Enums.ResponseStatus.Success)
                {
                    return CreatedAtAction(nameof(GetChatSession), new { sessionId = response.Data!.ChatSessionId }, response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat session at {Timestamp}", DateTime.UtcNow);
                return StatusCode(500, new ApiResponse<ChatSessionDTO>
                {
                    Status = BusinessObjects.Enums.ResponseStatus.Error,
                    Code = 500,
                    Errors = [new ApiError { Code = 5011, Message = "Internal server error while creating chat session" }]
                });
            }
        }

        /// <summary>
        /// Get user's chat sessions with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>List of chat sessions</returns>
        [HttpGet("sessions")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<ChatSessionDTO>>>> GetChatSessions(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Retrieving chat sessions for user at {Timestamp}", DateTime.UtcNow);
                var response = await _chatService.GetUserChatSessionsAsync(pageNumber, pageSize);

                if (response.Status == BusinessObjects.Enums.ResponseStatus.Success)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat sessions at {Timestamp}", DateTime.UtcNow);
                return StatusCode(500, new ApiResponse<List<ChatSessionDTO>>
                {
                    Status = BusinessObjects.Enums.ResponseStatus.Error,
                    Code = 500,
                    Errors = [new ApiError { Code = 5012, Message = "Internal server error while retrieving chat sessions" }]
                });
            }
        }

        /// <summary>
        /// Get a specific chat session with its messages
        /// </summary>
        /// <param name="sessionId">Chat session ID</param>
        /// <returns>Chat session with messages</returns>
        [HttpGet("session/{sessionId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ChatSessionDTO>>> GetChatSession(long sessionId)
        {
            try
            {
                _logger.LogInformation("Retrieving chat session {SessionId} at {Timestamp}", sessionId, DateTime.UtcNow);
                var response = await _chatService.GetChatSessionAsync(sessionId);

                if (response.Status == BusinessObjects.Enums.ResponseStatus.Success)
                {
                    return Ok(response);
                }

                if (response.Code == 404)
                {
                    return NotFound(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat session {SessionId} at {Timestamp}", sessionId, DateTime.UtcNow);
                return StatusCode(500, new ApiResponse<ChatSessionDTO>
                {
                    Status = BusinessObjects.Enums.ResponseStatus.Error,
                    Code = 500,
                    Errors = [new ApiError { Code = 5013, Message = "Internal server error while retrieving chat session" }]
                });
            }
        }

        /// <summary>
        /// Deactivate a chat session
        /// </summary>
        /// <param name="sessionId">Chat session ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("session/{sessionId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateChatSession(long sessionId)
        {
            try
            {
                _logger.LogInformation("Deactivating chat session {SessionId} at {Timestamp}", sessionId, DateTime.UtcNow);
                var response = await _chatService.DeactivateChatSessionAsync(sessionId);

                if (response.Status == BusinessObjects.Enums.ResponseStatus.Success)
                {
                    return Ok(response);
                }

                if (response.Code == 404)
                {
                    return NotFound(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating chat session {SessionId} at {Timestamp}", sessionId, DateTime.UtcNow);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Status = BusinessObjects.Enums.ResponseStatus.Error,
                    Code = 500,
                    Data = false,
                    Errors = [new ApiError { Code = 5014, Message = "Internal server error while deactivating chat session" }]
                });
            }
        }

        /// <summary>
        /// Get user context for AI personalization
        /// </summary>
        /// <returns>User learning context</returns>
        [HttpGet("user-context")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserContextDTO>>> GetUserContext()
        {
            try
            {
                _logger.LogInformation("Retrieving user context at {Timestamp}", DateTime.UtcNow);
                var response = await _chatService.GetUserContextAsync();

                if (response.Status == BusinessObjects.Enums.ResponseStatus.Success)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user context at {Timestamp}", DateTime.UtcNow);
                return StatusCode(500, new ApiResponse<UserContextDTO>
                {
                    Status = BusinessObjects.Enums.ResponseStatus.Error,
                    Code = 500,
                    Errors = [new ApiError { Code = 5015, Message = "Internal server error while retrieving user context" }]
                });
            }
        }

        /// <summary>
        /// Get the current AI system prompt for debugging/customization
        /// </summary>
        /// <returns>Current system prompt</returns>
        [HttpGet("system-prompt")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<string>>> GetSystemPrompt()
        {
            try
            {
                _logger.LogInformation("Retrieving system prompt at {Timestamp}", DateTime.UtcNow);
                
                // Get user context to build personalized prompt
                var userContextResponse = await _chatService.GetUserContextAsync();
                var systemPrompt = _aiService.BuildSystemPrompt(userContextResponse.Data);

                return Ok(new ApiResponse<string>
                {
                    Status = BusinessObjects.Enums.ResponseStatus.Success,
                    Code = 200,
                    Data = systemPrompt,
                    Message = "System prompt retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system prompt at {Timestamp}", DateTime.UtcNow);
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = BusinessObjects.Enums.ResponseStatus.Error,
                    Code = 500,
                    Errors = [new ApiError { Code = 5017, Message = "Internal server error while retrieving system prompt" }]
                });
            }
        }

        /// <summary>
        /// Check AI service availability
        /// </summary>
        /// <returns>AI service status</returns>
        [HttpGet("ai-status")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<object>>> GetAIServiceStatus()
        {
            try
            {
                _logger.LogInformation("Checking AI service status at {Timestamp}", DateTime.UtcNow);
                var isAvailable = await _aiService.IsServiceAvailableAsync();

                return Ok(new ApiResponse<object>
                {
                    Status = BusinessObjects.Enums.ResponseStatus.Success,
                    Code = 200,
                    Data = new 
                    { 
                        IsAvailable = isAvailable,
                        ServiceUrl = "http://127.0.0.1:1234",
                        ModelName = "qwen2.5-7b-instruct",
                        CheckedAt = DateTime.UtcNow
                    },
                    Message = isAvailable ? "AI service is available" : "AI service is not available"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking AI service status at {Timestamp}", DateTime.UtcNow);
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = BusinessObjects.Enums.ResponseStatus.Error,
                    Code = 500,
                    Errors = [new ApiError { Code = 5018, Message = "Error checking AI service status" }]
                });
            }
        }

        /// <summary>
        /// Get comprehensive learning analytics for AI personalization
        /// </summary>
        /// <returns>Detailed learning analytics</returns>
        [HttpGet("learning-analytics")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserLearningAnalyticsDTO>>> GetLearningAnalytics()
        {
            try
            {
                _logger.LogInformation("Retrieving learning analytics at {Timestamp}", DateTime.UtcNow);
                var response = await _chatService.GetLearningAnalyticsAsync();

                if (response.Status == BusinessObjects.Enums.ResponseStatus.Success)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving learning analytics at {Timestamp}", DateTime.UtcNow);
                return StatusCode(500, new ApiResponse<UserLearningAnalyticsDTO>
                {
                    Status = BusinessObjects.Enums.ResponseStatus.Error,
                    Code = 500,
                    Errors = [new ApiError { Code = 5016, Message = "Internal server error while retrieving learning analytics" }]
                });
            }
        }

        /// <summary>
        /// Test endpoint to verify authentication and user ID parsing
        /// </summary>
        [HttpGet("test-auth")]
        [Authorize]
        public ActionResult<object> TestAuth()
        {
            try
            {
                var userId = _currentUserService.AccountId;
                var email = _currentUserService.Email;
                var role = _currentUserService.Role;
                var status = _currentUserService.Status;

                return Ok(new
                {
                    UserId = userId,
                    Email = email,
                    Role = role,
                    Status = status,
                    Message = "Authentication successful"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message,
                    Message = "Authentication failed"
                });
            }
        }

    /// <summary>
    /// Test endpoint to verify user authentication and basic data
    /// </summary>
    [HttpGet("test-user")]
    [Authorize]
    public IActionResult TestUser()
    {
        try
        {
            var userId = _currentUserService.AccountId;
            var email = _currentUserService.Email;
            
            return Ok(new ApiResponse<object>
            {
                Status = ResponseStatus.Success,
                Code = 200,
                Message = "User authentication test successful",
                Data = new 
                {
                    UserId = userId,
                    Email = email,
                    Timestamp = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test user endpoint");
            return BadRequest(new ApiResponse<object>
            {
                Status = ResponseStatus.Error,
                Code = 400,
                Message = $"Error: {ex.Message}"
            });
        }
    }
    }
}
