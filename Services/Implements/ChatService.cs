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
    // DTO for fast course keyword matching
    public class CourseMatchDTO
    {
        public long CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = new List<string>();
        public int MatchScore { get; set; } = 0;
    }

    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IAIService _aiService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ChatService> _logger;
        private readonly ICourseService _courseService;
        private readonly IUserCourseInfoService _userCourseInfoService;

        // Cache for platform courses to improve performance
        private static List<CourseProgressDTO>? _cachedPlatformCourses = null;
        private static DateTime _cacheLastUpdated = DateTime.MinValue;
        private static readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);
        
        // Enhanced keyword cache for fast course matching
        private static Dictionary<string, List<CourseMatchDTO>>? _cachedCourseKeywords = null;
        private static List<string>? _cachedRegionalKeywords = null;

        public ChatService(
            IChatRepository chatRepository,
            IAIService aiService,
            ICurrentUserService currentUserService,
            ILogger<ChatService> logger,
            ICourseService courseService,
            IUserCourseInfoService userCourseInfoService)
        {
            _chatRepository = chatRepository;
            _aiService = aiService;
            _currentUserService = currentUserService;
            _logger = logger;
            _courseService = courseService;
            _userCourseInfoService = userCourseInfoService;
        }

        public async Task<ApiResponse<ChatMessageResponseDTO>> SendMessageAsync(ChatMessageRequestDTO request)
        {
            try
            {
                // Try to get user ID safely - use try/catch for guests
                long? userId = null;
                try
                {
                    userId = _currentUserService.AccountId;
                }
                catch (UnauthorizedAccessException)
                {
                    // Guest user - userId will remain null
                    userId = null;
                }

                return await SendMessageInternalAsync(request, userId);
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
                        ChatSessionId = chatSession?.ChatSessionId ?? 0, // Use 0 for guests to distinguish from -1
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
                long? userId = null;
                try
                {
                    userId = _currentUserService.AccountId;
                }
                catch (UnauthorizedAccessException)
                {
                    return new ApiResponse<ChatSessionDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Errors = [new ApiError { Code = 5010, Message = "Authentication required to create chat sessions" }]
                    };
                }
                
                var chatSession = new ChatSession
                {
                    UserId = userId.Value,
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
                long? userId = null;
                try
                {
                    userId = _currentUserService.AccountId;
                }
                catch (UnauthorizedAccessException)
                {
                    return new ApiResponse<List<ChatSessionDTO>>
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Errors = [new ApiError { Code = 5011, Message = "Authentication required to get chat sessions" }]
                    };
                }
                
                var sessions = await _chatRepository.GetUserChatSessionsAsync(userId.Value, pageNumber, pageSize);

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
                long? userId = null;
                try
                {
                    userId = _currentUserService.AccountId;
                }
                catch (UnauthorizedAccessException)
                {
                    return new ApiResponse<ChatSessionDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Errors = [new ApiError { Code = 5012, Message = "Authentication required to get chat session" }]
                    };
                }
                
                var session = await _chatRepository.GetChatSessionByIdAsync(chatSessionId, userId.Value);

                if (session == null)
                {
                    return new ApiResponse<ChatSessionDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Errors = [new ApiError { Code = 5004, Message = "Chat session not found" }]
                    };
                }

                var messages = await _chatRepository.GetChatMessagesAsync(chatSessionId, userId.Value);
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
                long? userId = null;
                try
                {
                    userId = _currentUserService.AccountId;
                }
                catch (UnauthorizedAccessException)
                {
                    return new ApiResponse<bool>
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Data = false,
                        Errors = [new ApiError { Code = 5013, Message = "Authentication required to deactivate chat session" }]
                    };
                }
                
                var result = await _chatRepository.DeactivateChatSessionAsync(chatSessionId, userId.Value);

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
                long? userId = null;
                try
                {
                    userId = _currentUserService.AccountId;
                }
                catch (UnauthorizedAccessException)
                {
                    return new ApiResponse<UserContextDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Errors = [new ApiError { Code = 5014, Message = "Authentication required to get user context" }]
                    };
                }
                
                var userContext = await _chatRepository.GetUserContextAsync(userId.Value);

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

        public async Task<ApiResponse<UserContextDTO>> GetUserContextAsync(long? userId)
        {
            try
            {
                UserContextDTO userContext;
                
                if (userId.HasValue)
                {
                    // Authenticated user - get personalized context with course data
                    userContext = await _chatRepository.GetUserContextAsync(userId.Value);
                    
                    // Enhanced: Add user's enrolled courses for better AI context
                    try
                    {
                        var enrolledCoursesResponse = await _courseService.GetCoursesHasEnrolledByUserIdAsync(userId.Value);
                        if (enrolledCoursesResponse.Status == ResponseStatus.Success && enrolledCoursesResponse.Data != null)
                        {
                            // Convert to CourseProgressDTO format for AI context
                            var enrolledCourses = enrolledCoursesResponse.Data.Select(course => new CourseProgressDTO
                            {
                                CourseId = course.CourseId,
                                CourseName = course.CourseTitle ?? "Unknown Course",
                                ProgressPercentage = 0, // This could be calculated from modules completion
                                CurrentModule = course.Modules?.FirstOrDefault()?.ModuleId.ToString() ?? "Not Started",
                                LastAccessed = course.CompletedAt ?? DateTime.UtcNow,
                                CurrentStreak = 0, // Could be enhanced with streak data
                                RecentTopics = new List<string>()
                            }).ToList();
                            
                            userContext.EnrolledCourses = enrolledCourses;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Could not load enrolled courses for user {UserId}: {Error}", userId.Value, ex.Message);
                        // Continue without enrolled courses data
                    }
                }
                else
                {
                    // Guest user - return comprehensive platform context for AI recommendations
                    userContext = new UserContextDTO
                    {
                        UserId = 0,
                        FullName = "Guest User",
                        Email = "guest@artjourney.com",
                        EnrolledCourses = new List<CourseProgressDTO>(),
                        CompletedTopics = new List<string> { "Europe", "Asia", "Africa", "North America", "South America", "Oceania" },
                        StruggleAreas = new List<string>(),
                        PreferredLearningStyle = "Visual",
                        InterestAreas = new List<string> 
                        { 
                            // Core art categories
                            "Art History", "Classical Art", "Renaissance", "Modern Art", "Contemporary Art",
                            "Cultural Heritage", "Traditional Art", "Digital Art", "Sculpture", "Painting",
                            
                            // Regional art styles - key for matching user queries
                            "Vietnamese Art", "Chinese Art", "Japanese Art", "Korean Art", "Asian Culture",
                            "European Art", "Italian Renaissance", "French Art", "German Art", "Spanish Art",
                            "African Art", "Egyptian Art", "Tribal Art", "Ancient African Culture",
                            "American Art", "Native American Art", "Colonial Art", "Modern American Art",
                            "Latin American Art", "Brazilian Art", "Mayan Art", "Aztec Art", "Inca Art",
                            "Australian Art", "Aboriginal Art", "Pacific Art", "Polynesian Culture",
                            
                            // Art movements and styles
                            "Baroque", "Rococo", "Neoclassicism", "Romanticism", "Impressionism", 
                            "Post-Impressionism", "Cubism", "Surrealism", "Abstract Art",
                            
                            // Cultural and historical contexts
                            "Ancient Civilizations", "Medieval Art", "Islamic Art", "Buddhist Art",
                            "Religious Art", "Folk Art", "Decorative Arts", "Architecture"
                        },
                        CurrentDateTime = DateTime.UtcNow,
                        TimeZone = "UTC"
                    };
                    
                    // Enhanced: Add ALL platform courses with regional context for comprehensive AI recommendations
                    try
                    {
                        // Ensure keyword cache is populated for fast recommendations
                        await EnsureCourseKeywordCacheAsync();
                        
                        var allCoursesResponse = await _courseService.GetAllPublishedCoursesGroupedByRegionAsync();
                        if (allCoursesResponse.Status == ResponseStatus.Success && allCoursesResponse.Data != null)
                        {
                            var platformCourses = new List<CourseProgressDTO>();
                            var platformRegions = new List<string>();
                            var historicalPeriods = new List<string>();
                            var culturalKeywords = new List<string>();
                            
                            foreach (var region in allCoursesResponse.Data)
                            {
                                if (!string.IsNullOrEmpty(region.RegionName))
                                    platformRegions.Add(region.RegionName);
                                
                                // Add historical periods for this region
                                historicalPeriods.AddRange(region.historicalPeriodDTOs
                                    .Where(h => !string.IsNullOrEmpty(h.HistoricalPeriodName))
                                    .Select(h => h.HistoricalPeriodName));
                                
                                // Add courses with enhanced keyword matching for AI
                                foreach (var course in region.Courses)
                                {
                                    var courseKeywords = BuildCourseKeywords(course.Title, region.RegionName, course.Description);
                                    culturalKeywords.AddRange(courseKeywords.Take(5)); // Top keywords only
                                    
                                    platformCourses.Add(new CourseProgressDTO
                                    {
                                        CourseId = course.CourseId,
                                        CourseName = course.Title,
                                        ProgressPercentage = 0,
                                        CurrentModule = $"🌍 {region.RegionName} Region - Available for Enrollment",
                                        LastAccessed = DateTime.UtcNow,
                                        CurrentStreak = 0,
                                        RecentTopics = courseKeywords.Distinct().Take(8).ToList()
                                    });
                                }
                            }
                            
                            userContext.EnrolledCourses = platformCourses.Take(30).ToList(); // Optimized for comprehensive coverage
                            userContext.InterestAreas.AddRange(platformRegions.Distinct());
                            userContext.InterestAreas.AddRange(historicalPeriods.Distinct().Take(15));
                            userContext.InterestAreas.AddRange(culturalKeywords.Distinct().Take(25)); // Enhanced cultural keywords
                            
                            // Add optimized platform summary for faster AI processing
                            userContext.CompletedTopics.Add($"🎨 ArtJourney has {platformCourses.Count} courses in {platformRegions.Distinct().Count()} regions");
                            userContext.CompletedTopics.Add($"📚 Regions: {string.Join(", ", platformRegions.Distinct())}");
                            userContext.CompletedTopics.Add($"⏰ Historical Periods: {string.Join(", ", historicalPeriods.Distinct().Take(8))}");
                            userContext.CompletedTopics.Add($"🔍 Fast Course Matching: Ready for {_cachedCourseKeywords?.Count ?? 0} keyword patterns");
                            userContext.CompletedTopics.Add($"⚡ Performance: Course data cached at {_cacheLastUpdated:HH:mm:ss}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Could not load platform courses for guest user: {Error}", ex.Message);
                        // Enhanced fallback with regional course mapping hints
                        userContext.CompletedTopics.Add("🎨 ArtJourney Platform: 6 regions with specialized art courses");
                        userContext.CompletedTopics.Add("🌍 Europe: Renaissance, Classical Art | Asia: Vietnamese, Chinese, Japanese Art");
                        userContext.CompletedTopics.Add("🌍 Africa: Ancient, Tribal Art | Americas: Native, Colonial Art | Oceania: Pacific Art");
                        userContext.CompletedTopics.Add("💰 3 PRICING OPTIONS: FREE (limited), PREMIUM (90K VND/month), PAY-PER-COURSE");
                        userContext.InterestAreas.AddRange(new[] { "Vietnamese Art", "Chinese Art", "Renaissance", "Classical Art", "Modern Art", "Cultural Heritage" });
                    }
                }

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
                _logger.LogError(ex, "Error getting user context for userId: {UserId}", userId);
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
                long? userId = null;
                try
                {
                    userId = _currentUserService.AccountId;
                }
                catch (UnauthorizedAccessException)
                {
                    return new ApiResponse<UserLearningAnalyticsDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Errors = [new ApiError { Code = 5015, Message = "Authentication required to get learning analytics" }]
                    };
                }
                
                var analytics = await _chatRepository.GetLearningAnalyticsAsync(userId.Value);

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
            try
            {
                // Clean the message
                var cleanMessage = firstMessage?.Trim() ?? "";
                
                if (string.IsNullOrWhiteSpace(cleanMessage))
                {
                    return GetRandomFallbackTitle();
                }

                // Detect art-related topics for smarter titles
                string title = GenerateContextualTitle(cleanMessage);
                
                if (!string.IsNullOrEmpty(title))
                {
                    return title;
                }

                // Remove common punctuation and normalize
                cleanMessage = System.Text.RegularExpressions.Regex.Replace(cleanMessage, @"[^\w\s]", "").Trim();
                
                // Enhanced stop words list
                var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "the", "and", "for", "are", "but", "not", "you", "all", "can", "had", "her", "was", "one", "our", "out", "day", "get", "has", "him", "his", "how", "its", "may", "new", "now", "old", "see", "two", "who", "boy", "did", "she", "use", "way", "what", "with", "this", "that", "they", "them", "have", "from", "will", "been", "said", "each", "which", "their", "time", "about", "would", "there", "could", "other", "after", "first", "well", "want", "just", "know", "also", "back", "where", "much", "before", "here", "through", "when", "come", "some", "into", "than", "more", "very", "like", "over", "think", "even", "most", "should", "being", "tell", "help", "make", "give", "take", "work", "look", "find", "good", "great", "little", "right", "still", "while", "then", "such", "many", "only", "need", "feel", "seem", "ask", "show", "try", "turn", "move", "live", "might", "never", "again", "last", "long", "keep", "leave", "let", "put"
                };
                
                // Split into words and filter meaningful ones
                var words = cleanMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 2) // Filter out short words
                    .Where(w => !stopWords.Contains(w))
                    .Where(w => !System.Text.RegularExpressions.Regex.IsMatch(w, @"^\d+$")) // Remove pure numbers
                    .Take(4) // Take up to 4 meaningful words
                    .ToList();

                if (words.Any())
                {
                    title = string.Join(" ", words);
                    
                    // Capitalize first letter of each word
                    title = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
                    
                    // Add contextual prefixes based on content
                    title = AddContextualPrefix(title, firstMessage ?? "");
                }
                else
                {
                    // Fallback to first few words if no meaningful words found
                    var firstWords = (firstMessage ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(3);
                    title = string.Join(" ", firstWords);
                    title = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
                }

                // Ensure reasonable length
                if (title.Length > 50)
                {
                    title = title.Substring(0, 47) + "...";
                }
                else if (title.Length < 8)
                {
                    title = GetRandomFallbackTitle();
                }

                return title;
            }
            catch
            {
                // Fallback title if anything goes wrong
                return GetRandomFallbackTitle();
            }
        }

        private string GenerateContextualTitle(string message)
        {
            var lowerMessage = message.ToLower();
            
            // Art history periods and movements
            var artPeriods = new Dictionary<string, string>
            {
                {"renaissance", "🎨 Renaissance Art"},
                {"baroque", "✨ Baroque Period"},
                {"impressionism", "🌅 Impressionist Art"},
                {"cubism", "🔷 Cubist Movement"},
                {"surrealism", "🎭 Surrealist Art"},
                {"modernism", "🏛️ Modern Art"},
                {"contemporary", "🆕 Contemporary Art"},
                {"abstract", "🎨 Abstract Art"},
                {"realism", "📸 Realist Art"},
                {"expressionism", "💭 Expressionist Art"},
                {"romanticism", "💫 Romantic Period"},
                {"neoclassicism", "🏛️ Neoclassical Art"},
                {"gothic", "⛪ Gothic Art"},
                {"byzantine", "👑 Byzantine Art"},
                {"medieval", "🏰 Medieval Art"}
            };

            // Art techniques and mediums
            var artTechniques = new Dictionary<string, string>
            {
                {"painting", "🖼️ Painting Techniques"},
                {"sculpture", "🗿 Sculpture Art"},
                {"drawing", "✏️ Drawing Skills"},
                {"watercolor", "🎨 Watercolor Art"},
                {"oil painting", "🖌️ Oil Painting"},
                {"digital art", "💻 Digital Art"},
                {"photography", "📷 Photography"},
                {"printmaking", "🖨️ Printmaking"},
                {"ceramics", "🏺 Ceramic Art"},
                {"textile", "🧵 Textile Art"}
            };

            // Famous artists
            var artists = new Dictionary<string, string>
            {
                {"da vinci", "🎨 Leonardo da Vinci"},
                {"michelangelo", "🗿 Michelangelo"},
                {"picasso", "🔷 Pablo Picasso"},
                {"van gogh", "🌻 Vincent van Gogh"},
                {"monet", "🌅 Claude Monet"},
                {"dali", "🎭 Salvador Dalí"},
                {"kahlo", "🌺 Frida Kahlo"},
                {"warhol", "🎨 Andy Warhol"},
                {"basquiat", "👑 Jean-Michel Basquiat"},
                {"banksy", "🎨 Banksy"}
            };

            // Question-based titles
            var questionPatterns = new Dictionary<string, string>
            {
                {"what is", "❓ Understanding"},
                {"how to", "🔧 Learning"},
                {"why", "🤔 Exploring"},
                {"when", "📅 Timeline"},
                {"where", "📍 Location"},
                {"who", "👤 Artist Study"},
                {"which", "🎯 Comparison"},
                {"can you", "💡 Assistance"},
                {"tell me", "📚 Information"},
                {"explain", "📖 Explanation"},
                {"help", "🆘 Help Request"},
                {"learn", "📚 Learning"},
                {"practice", "💪 Practice"},
                {"improve", "📈 Improvement"}
            };

            // Check for art periods
            foreach (var period in artPeriods)
            {
                if (lowerMessage.Contains(period.Key))
                {
                    return period.Value;
                }
            }

            // Check for art techniques
            foreach (var technique in artTechniques)
            {
                if (lowerMessage.Contains(technique.Key))
                {
                    return technique.Value;
                }
            }

            // Check for artists
            foreach (var artist in artists)
            {
                if (lowerMessage.Contains(artist.Key))
                {
                    return artist.Value;
                }
            }

            // Check for question patterns
            foreach (var pattern in questionPatterns)
            {
                if (lowerMessage.StartsWith(pattern.Key))
                {
                    return pattern.Value;
                }
            }

            return string.Empty;
        }

        private string AddContextualPrefix(string title, string originalMessage)
        {
            var lowerMessage = originalMessage.ToLower();
            
            if (lowerMessage.Contains("?"))
            {
                return $"❓ {title}";
            }
            
            if (lowerMessage.Contains("help") || lowerMessage.Contains("assistance"))
            {
                return $"🆘 {title}";
            }
            
            if (lowerMessage.Contains("learn") || lowerMessage.Contains("teach") || lowerMessage.Contains("study"))
            {
                return $"📚 {title}";
            }
            
            if (lowerMessage.Contains("create") || lowerMessage.Contains("make") || lowerMessage.Contains("draw") || lowerMessage.Contains("paint"))
            {
                return $"🎨 {title}";
            }
            
            if (lowerMessage.Contains("history") || lowerMessage.Contains("period") || lowerMessage.Contains("era"))
            {
                return $"📜 {title}";
            }
            
            if (lowerMessage.Contains("technique") || lowerMessage.Contains("method") || lowerMessage.Contains("skill"))
            {
                return $"🔧 {title}";
            }
            
            return title;
        }

        private string GetRandomFallbackTitle()
        {
            var fallbackTitles = new[]
            {
                "🎨 Art Discovery",
                "✨ Creative Journey",
                "🖼️ Art Exploration",
                "📚 Learning Session", 
                "💡 Art Insights",
                "🌟 Creative Chat",
                "🎭 Art Adventure",
                "🖌️ Artistic Discussion",
                "📖 Knowledge Quest",
                "🔍 Art Investigation"
            };
            
            var random = new Random();
            return fallbackTitles[random.Next(fallbackTitles.Length)];
        }

        private async Task<ApiResponse<ChatMessageResponseDTO>> SendMessageInternalAsync(ChatMessageRequestDTO request, long? userId)
        {
            var currentTimestamp = DateTime.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            // Create or get chat session (only for authenticated users)
            ChatSession? chatSession = null;
            if (userId.HasValue)
            {
                if (request.ChatSessionId.HasValue)
                {
                    chatSession = await _chatRepository.GetChatSessionByIdAsync(request.ChatSessionId.Value, userId.Value);
                }

                if (chatSession == null)
                {
                    chatSession = new ChatSession
                    {
                        UserId = userId.Value,
                        SessionTitle = GenerateSessionTitle(request.Message),
                        CreatedAt = currentTimestamp,
                        UpdatedAt = currentTimestamp,
                        IsActive = true
                    };
                    chatSession = await _chatRepository.CreateChatSessionAsync(chatSession);
                }

                // Save user message for authenticated users
                var userMessage = new ChatMessage
                {
                    ChatSessionId = chatSession.ChatSessionId,
                    Role = "user",
                    Content = request.Message,
                    Timestamp = currentTimestamp
                };
                await _chatRepository.CreateChatMessageAsync(userMessage);
            }

            // Get user context - for authenticated users if requested, for guests always (to provide platform data)
            UserContextDTO? userContext = null;
            if ((request.IncludeUserProgress && userId.HasValue) || !userId.HasValue)
            {
                var contextResponse = await GetUserContextAsync(userId);
                if (contextResponse.Status == ResponseStatus.Success && contextResponse.Data != null)
                {
                    userContext = contextResponse.Data;
                    userContext.CurrentDateTime = currentTimestamp;
                    
                    // Add platform-specific messaging for guests
                    if (!userId.HasValue)
                    {
                        userContext.CompletedTopics.Add("🎨 You're chatting with ArtJourney's AI assistant!");
                        userContext.CompletedTopics.Add("💡 I can help you find the perfect art course from our platform");
                        userContext.CompletedTopics.Add("🌍 Our 6 regions: Europe, Asia, Africa, North America, South America, Oceania");
                        userContext.CompletedTopics.Add("🔍 Ask me about specific cultures (e.g., 'Vietnamese art'), historical periods, or art styles");
                        userContext.CompletedTopics.Add("📚 I'll recommend actual ArtJourney courses that match your interests");
                        userContext.CompletedTopics.Add("⚡ For faster responses, be specific about what art or culture you're interested in");
                        
                        // Enhance with fast course matching results
                        try
                        {
                            var matchedCourses = await FindMatchingCoursesAsync(request.Message, 5);
                            if (matchedCourses.Any())
                            {
                                userContext.CompletedTopics.Add($"🎯 Found {matchedCourses.Count} matching courses for your query:");
                                foreach (var match in matchedCourses.Take(3))
                                {
                                    userContext.CompletedTopics.Add($"   📖 {match.CourseName} ({match.RegionName}) - Score: {match.MatchScore}");
                                }
                                userContext.CompletedTopics.Add("💯 I'll prioritize these courses in my response!");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Fast course matching failed: {Error}", ex.Message);
                        }
                    }
                }
            }

            // Get chat history (only for authenticated users with sessions)
            List<ChatMessageResponseDTO> chatHistory = new List<ChatMessageResponseDTO>();
            if (chatSession != null && userId.HasValue)
            {
                chatHistory = await GetChatHistoryAsync(chatSession.ChatSessionId, userId.Value);
            }

            // Generate AI response
            var aiResponseContent = await _aiService.GenerateResponseAsync(
                request.Message,
                userContext,
                chatHistory);

            // Enhance AI response with fast course matching if the response doesn't already include specific recommendations
            try
            {
                var userMessage = request.Message.ToLower();
                if (!aiResponseContent.Contains("📚") && !aiResponseContent.Contains("Course") && 
                    (userMessage.Contains("art") || userMessage.Contains("culture") || userMessage.Contains("paint") || 
                     userMessage.Contains("history") || userMessage.Contains("learn") || userMessage.Contains("course")))
                {
                    var aiRecommendationContext = await GenerateAIRecommendationContextAsync(request.Message, userId);
                    
                    // Intelligently append recommendations
                    if (!string.IsNullOrEmpty(aiRecommendationContext) && !aiRecommendationContext.Contains("Tell me what interests"))
                    {
                        aiResponseContent += $"\n\n{aiRecommendationContext}";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to enhance AI response with course recommendations: {Error}", ex.Message);
            }

            stopwatch.Stop();

            // Save AI response for authenticated users with sessions
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

                // Update session last activity
                chatSession.UpdatedAt = aiResponseTimestamp;
                await _chatRepository.UpdateChatSessionAsync(chatSession);
            }

            var responseDto = new ChatMessageResponseDTO
            {
                ChatMessageId = savedAiMessage?.ChatMessageId ?? 0,
                ChatSessionId = chatSession?.ChatSessionId ?? 0,
                Role = "assistant",
                Content = aiResponseContent,
                Timestamp = DateTime.UtcNow,
                ModelUsed = "LM Studio Local",
                ResponseTime = stopwatch.Elapsed.TotalSeconds
            };

            return new ApiResponse<ChatMessageResponseDTO>
            {
                Status = ResponseStatus.Success,
                Code = 200,
                Data = responseDto,
                Message = userId.HasValue ? "Message sent successfully" : "Guest message processed successfully"
            };
        }

        /// <summary>
        /// Fast keyword-based course matching for instant recommendations
        /// </summary>
        private async Task<List<CourseMatchDTO>> FindMatchingCoursesAsync(string userQuery, int maxResults = 5)
        {
            try
            {
                // Ensure cache is populated
                await EnsureCourseKeywordCacheAsync();
                
                if (_cachedCourseKeywords == null || !_cachedCourseKeywords.Any())
                    return new List<CourseMatchDTO>();

                var queryWords = userQuery.ToLower()
                    .Split(new[] { ' ', ',', '.', '!', '?', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(word => word.Length > 2) // Filter out short words
                    .ToList();

                var matchedCourses = new List<CourseMatchDTO>();
                
                // Fast keyword matching
                foreach (var keyword in queryWords)
                {
                    if (_cachedCourseKeywords.ContainsKey(keyword))
                    {
                        foreach (var course in _cachedCourseKeywords[keyword])
                        {
                            var existingMatch = matchedCourses.FirstOrDefault(m => m.CourseId == course.CourseId);
                            if (existingMatch != null)
                            {
                                existingMatch.MatchScore += 2; // Boost score for multiple keyword matches
                            }
                            else
                            {
                                matchedCourses.Add(new CourseMatchDTO
                                {
                                    CourseId = course.CourseId,
                                    CourseName = course.CourseName,
                                    RegionName = course.RegionName,
                                    Keywords = course.Keywords,
                                    MatchScore = 1
                                });
                            }
                        }
                    }
                }
                
                // Enhanced cultural/regional pattern matching
                var enhancedMatches = await EnhanceCulturalMatchingAsync(userQuery, matchedCourses);
                
                return enhancedMatches
                    .OrderByDescending(m => m.MatchScore)
                    .Take(maxResults)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error in fast course matching: {Error}", ex.Message);
                return new List<CourseMatchDTO>();
            }
        }

        /// <summary>
        /// Enhanced cultural and regional pattern matching
        /// </summary>
        private Task<List<CourseMatchDTO>> EnhanceCulturalMatchingAsync(string userQuery, List<CourseMatchDTO> existingMatches)
        {
            var query = userQuery.ToLower();
            var culturalPatterns = new Dictionary<string, string[]>
            {
                ["vietnamese"] = new[] { "vietnam", "vietnamese", "southeast asia", "asian" },
                ["chinese"] = new[] { "china", "chinese", "mandarin", "asian", "east asia" },
                ["japanese"] = new[] { "japan", "japanese", "asian", "east asia", "samurai", "zen" },
                ["european"] = new[] { "europe", "european", "western", "renaissance", "baroque", "classical" },
                ["renaissance"] = new[] { "renaissance", "italian", "european", "classical", "leonardo", "michelangelo" },
                ["impressionist"] = new[] { "impressionism", "monet", "renoir", "french", "european" },
                ["ancient"] = new[] { "ancient", "classical", "greek", "roman", "egyptian", "mesopotamian" },
                ["african"] = new[] { "africa", "african", "tribal", "traditional", "ancient egypt" },
                ["american"] = new[] { "america", "american", "native american", "colonial", "modern" },
                ["aboriginal"] = new[] { "aboriginal", "australian", "pacific", "oceania", "indigenous" },
                ["contemporary"] = new[] { "contemporary", "modern", "current", "21st century", "digital" },
                ["traditional"] = new[] { "traditional", "folk", "cultural", "heritage", "ancient" }
            };

            // Apply cultural pattern matching and boost scores
            foreach (var pattern in culturalPatterns)
            {
                if (pattern.Value.Any(term => query.Contains(term)))
                {
                    // Boost existing matches that align with this cultural pattern
                    foreach (var match in existingMatches)
                    {
                        if (match.Keywords.Any(k => pattern.Value.Contains(k.ToLower())))
                        {
                            match.MatchScore += 3; // Cultural context boost
                        }
                    }
                }
            }

            return Task.FromResult(existingMatches);
        }

        /// <summary>
        /// Ensure course keyword cache is populated and up-to-date
        /// </summary>
        private async Task EnsureCourseKeywordCacheAsync()
        {
            if (_cachedCourseKeywords != null && 
                _cacheLastUpdated > DateTime.UtcNow.Subtract(_cacheExpiry))
            {
                return; // Cache is still valid
            }

            try
            {
                var allCoursesResponse = await _courseService.GetAllPublishedCoursesGroupedByRegionAsync();
                if (allCoursesResponse.Status != ResponseStatus.Success || allCoursesResponse.Data == null)
                    return;

                var keywordCache = new Dictionary<string, List<CourseMatchDTO>>();
                var regionalKeywords = new List<string>();

                foreach (var region in allCoursesResponse.Data)
                {
                    if (!string.IsNullOrEmpty(region.RegionName))
                        regionalKeywords.Add(region.RegionName);

                    foreach (var course in region.Courses)
                    {
                        var courseMatch = new CourseMatchDTO
                        {
                            CourseId = course.CourseId,
                            CourseName = course.Title,
                            RegionName = region.RegionName ?? "Art",
                            Keywords = BuildCourseKeywords(course.Title, region.RegionName, course.Description)
                        };

                        // Index all keywords for fast lookup
                        foreach (var keyword in courseMatch.Keywords)
                        {
                            var key = keyword.ToLower();
                            if (!keywordCache.ContainsKey(key))
                                keywordCache[key] = new List<CourseMatchDTO>();
                            

                            keywordCache[key].Add(courseMatch);
                        }
                    }
                }

                _cachedCourseKeywords = keywordCache;
                _cachedRegionalKeywords = regionalKeywords;
                _cacheLastUpdated = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to build course keyword cache: {Error}", ex.Message);
            }
        }

        /// <summary>
        /// Build comprehensive keyword list for a course
        /// </summary>
        private List<string> BuildCourseKeywords(string title, string? regionName, string? description)
        {
            var keywords = new List<string>();
            
            // Title words
            if (!string.IsNullOrEmpty(title))
            {
                keywords.AddRange(title.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }
            
            // Region-based keywords
            if (!string.IsNullOrEmpty(regionName))
            {
                keywords.Add(regionName);
                keywords.Add($"{regionName} Art");
                keywords.Add($"{regionName} Culture");
                
                // Add region-specific cultural keywords
                switch (regionName.ToLower())
                {
                    case "asia":
                        keywords.AddRange(new[] { "Asian", "Chinese", "Japanese", "Korean", "Vietnamese", "Thai", "Indian", "Eastern", "Orient", "Confucian", "Buddhist", "Hindu" });
                        break;
                    case "europe":
                        keywords.AddRange(new[] { "European", "Italian", "French", "German", "Spanish", "Renaissance", "Classical", "Western", "Baroque", "Gothic", "Medieval" });
                        break;
                    case "africa":
                        keywords.AddRange(new[] { "African", "Egyptian", "Tribal", "Traditional", "Ancient", "Saharan", "Sub-Saharan", "Ethiopian", "Nubian" });
                        break;
                    case "north america":
                        keywords.AddRange(new[] { "American", "Native American", "Canadian", "Colonial", "Modern American", "Indigenous", "Pre-Columbian" });
                        break;
                    case "south america":
                        keywords.AddRange(new[] { "Latin American", "Brazilian", "Argentinian", "Mayan", "Aztec", "Inca", "Pre-Columbian", "Colonial" });
                        break;
                    case "oceania":
                        keywords.AddRange(new[] { "Australian", "Pacific", "Aboriginal", "Polynesian", "Melanesian", "Micronesian", "Indigenous" });
                        break;
                }
            }
            
            // Description keywords (first 50 words to avoid bloat)
            if (!string.IsNullOrEmpty(description))
            {
                var descWords = description.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 3)
                    .Take(50);
                keywords.AddRange(descWords);
            }
            
            // Art-specific keywords for broader matching
            keywords.AddRange(new[] { "art", "culture", "painting", "sculpture", "history", "heritage", "artistic", "creative", "visual", "aesthetic" });
            
            return keywords.Distinct().ToList();
        }

        /// <summary>
        /// Generate AI-optimized course recommendations for better responses
        /// </summary>
        private async Task<string> GenerateAIRecommendationContextAsync(string userQuery, long? userId = null)
        {
            try
            {
                var matchedCourses = await FindMatchingCoursesAsync(userQuery, 8);
                if (!matchedCourses.Any())
                {
                    return "🎨 ArtJourney offers comprehensive art courses across 6 global regions. Ask about specific cultures, art movements, or historical periods for personalized recommendations.";
                }

                var recommendations = new List<string>
                {
                    $"🎯 Based on your query '{userQuery}', here are the top ArtJourney course matches:"
                };

                // Group by region for better organization
                var coursesByRegion = matchedCourses.GroupBy(c => c.RegionName).ToList();
                
                foreach (var regionGroup in coursesByRegion.Take(3)) // Top 3 regions
                {
                    var regionCourses = regionGroup.OrderByDescending(c => c.MatchScore).Take(3);
                    recommendations.Add($"\n🌍 **{regionGroup.Key} Region:**");
                    
                    foreach (var course in regionCourses)
                    {
                        var confidence = course.MatchScore >= 3 ? "🔥 Perfect match" : 
                                        course.MatchScore >= 2 ? "✨ Great match" : "👍 Good match";
                        recommendations.Add($"   📚 {course.CourseName} - {confidence}");
                    }
                }

                recommendations.Add("\n💡 I can provide detailed information about any of these courses, including content, duration, and learning outcomes.");
                recommendations.Add("🚀 Would you like to explore any specific course or region in more detail?");

                return string.Join("\n", recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to generate AI recommendation context: {Error}", ex.Message);
                return "🎨 I can help you discover amazing art courses on ArtJourney! Tell me what interests you - specific cultures, art styles, or regions.";
            }
        }

        /// <summary>
        /// Background method to refresh course cache periodically (can be called by a background service)
        /// </summary>
        public async Task RefreshCourseCacheAsync()
        {
            try
            {
                _logger.LogInformation("Refreshing course cache...");
                
                // Force cache refresh by clearing existing cache
                _cachedCourseKeywords = null;
                _cachedPlatformCourses = null;
                _cacheLastUpdated = DateTime.MinValue;
                
                // Rebuild cache
                await EnsureCourseKeywordCacheAsync();
                
                _logger.LogInformation("Course cache refreshed successfully. Keywords: {KeywordCount}, Last Updated: {LastUpdated}", 
                    _cachedCourseKeywords?.Count ?? 0, _cacheLastUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh course cache");
            }
        }

        /// <summary>
        /// Get cache statistics for monitoring
        /// </summary>
        public async Task<object> GetCacheStatsAsync()
        {
            await EnsureCourseKeywordCacheAsync();
            
            return new
            {
                KeywordPatterns = _cachedCourseKeywords?.Count ?? 0,
                RegionalKeywords = _cachedRegionalKeywords?.Count ?? 0,
                PlatformCourses = _cachedPlatformCourses?.Count ?? 0,
                LastUpdated = _cacheLastUpdated,
                CacheAge = DateTime.UtcNow.Subtract(_cacheLastUpdated),
                IsExpired = DateTime.UtcNow.Subtract(_cacheLastUpdated) > _cacheExpiry
            };
        }
    }
}
