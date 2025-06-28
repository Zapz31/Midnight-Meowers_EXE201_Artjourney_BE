using Helpers.DTOs.Chat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Services.Implements
{
    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIService> _logger;
        private readonly string _aiServiceUrl;
        private readonly string _modelName;
        private readonly int _maxTokens;
        private readonly double _temperature;
        private readonly int _contextLength;
        
        // New optimization parameters
        private readonly int _shortMessageTokens;
        private readonly int _shortMessageThreshold;
        private readonly double _fastTemperature;
        
        // Context limit management
        private readonly int _maxContextTokens;
        private readonly int _contextWarningThreshold;

        public AIService(HttpClient httpClient, ILogger<AIService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _aiServiceUrl = configuration.GetValue<string>("AIService:BaseUrl") ?? "https://yairozu.tail682e6a.ts.net/v1";
            _modelName = configuration.GetValue<string>("AIService:ModelName") ?? "qwen2.5-7b-instruct";
            _maxTokens = configuration.GetValue<int>("AIService:MaxTokens", 1500);
            _temperature = configuration.GetValue<double>("AIService:Temperature", 0.7);
            _contextLength = configuration.GetValue<int>("AIService:ContextLength", 10);
            
            // Optimization settings
            _shortMessageTokens = configuration.GetValue<int>("AIService:ShortMessageTokens", 500);
            _shortMessageThreshold = configuration.GetValue<int>("AIService:ShortMessageThreshold", 50);
            _fastTemperature = configuration.GetValue<double>("AIService:FastTemperature", 0.3);
            
            // Context limits
            _maxContextTokens = configuration.GetValue<int>("AIService:MaxContextTokens", 6000);
            _contextWarningThreshold = configuration.GetValue<int>("AIService:ContextWarningThreshold", 5000);
            
            var timeoutSeconds = configuration.GetValue<int>("AIService:Timeout", 60); // Reduced from 120
            _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        }

        public async Task<string> GenerateResponseAsync(string userMessage, UserContextDTO? userContext = null, List<ChatMessageResponseDTO>? chatHistory = null)
        {
                // Optimize for short messages
                var isShortMessage = userMessage.Length <= _shortMessageThreshold;
                var maxTokens = isShortMessage ? _shortMessageTokens : _maxTokens;
                var temperature = isShortMessage ? _fastTemperature : _temperature;
            try
            {
                var currentDateTime = DateTime.UtcNow;
                
                // Check for quiz content first
                if (IsQuizOrAssessmentContent(userMessage))
                {
                    return GetAntiCheatResponse(userContext);
                }
                
                
                // Check context limits and prepare history
                var (processedHistory, contextStatus) = PrepareContextWithLimits(chatHistory, isShortMessage);
                
                // For short messages, use minimal context
                var systemPrompt = isShortMessage ? 
                    BuildMinimalSystemPrompt(userContext) : 
                    BuildSystemPrompt(userContext);
                
                var messages = new List<object>
                {
                    new { role = "system", content = systemPrompt }
                };

                // Add processed chat history
                foreach (var msg in processedHistory)
                {
                    messages.Add(new { role = msg.Role, content = msg.Content });
                }

                // Add current user message with timestamp
                var timestampedMessage = $"[{currentDateTime:yyyy-MM-dd HH:mm:ss} UTC] {userMessage}";
                messages.Add(new { role = "user", content = timestampedMessage });

                var requestBody = new
                {
                    model = _modelName,
                    messages = messages,
                    temperature = temperature,
                    max_tokens = maxTokens,
                    stream = false,
                    presence_penalty = isShortMessage ? 0.0 : 0.1,
                    frequency_penalty = isShortMessage ? 0.0 : 0.1,
                    // Add these for better performance
                    top_p = 0.9,
                    stop = new[] { "\n\nUser:", "\n\nAssistant:" }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions 
                { 
                    WriteIndented = false 
                });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending {MessageType} request to AI service: {Length} chars, {MaxTokens} tokens, Context: {ContextStatus}", 
                    isShortMessage ? "optimized" : "full", userMessage.Length, maxTokens, contextStatus);

                var response = await _httpClient.PostAsync($"{_aiServiceUrl}/chat/completions", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("AI service returned {StatusCode}: {Content}", response.StatusCode, await response.Content.ReadAsStringAsync());
                    return GetFallbackResponse(userContext, isShortMessage);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var aiResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                if (aiResponse.TryGetProperty("choices", out var choices) && 
                    choices.GetArrayLength() > 0)
                {
                    var choice = choices[0];
                    if (choice.TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var aiContent))
                    {
                        var finalResponse = aiContent.GetString() ?? GetFallbackResponse(userContext, isShortMessage);
                        
                        // Add context warning if needed
                        if (contextStatus.Contains("warning") || contextStatus.Contains("limit"))
                        {
                            finalResponse += $"\n\n⚠️ **Context Notice**: {GetContextLimitMessage(contextStatus)}";
                        }
                        
                        return finalResponse;
                    }
                }

                _logger.LogWarning("Unexpected AI response format: {Response}", responseContent);
                return GetFallbackResponse(userContext, isShortMessage);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError("AI service request timed out: {Message}", ex.Message);
                return GetFallbackResponse(userContext, isShortMessage);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("AI service connection error: {Message}", ex.Message);
                return GetFallbackResponse(userContext, isShortMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in AI service");
                return GetFallbackResponse(userContext, isShortMessage);
            }
        }

        public async Task<bool> IsServiceAvailableAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_aiServiceUrl}/models");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public string BuildSystemPrompt(UserContextDTO? userContext = null)
        {
            var currentDateTime = DateTime.UtcNow;
            var prompt = new StringBuilder();
            
            prompt.AppendLine("You are ArtBot, a friendly AI learning assistant for ArtJourney, an innovative art history learning platform.");
            prompt.AppendLine("Your role is to guide learners through their art history education with personalized recommendations and warm support.");
            prompt.AppendLine();
            prompt.AppendLine("CORE PERSONALITY:");
            prompt.AppendLine("- Warm, welcoming, and enthusiastic about art history");
            prompt.AppendLine("- Always greet users by name (fullname if available, otherwise email username, or 'friend' for guests)");
            prompt.AppendLine("- Patient and adaptive to different learning styles");
            prompt.AppendLine("- Scholarly but accessible in your explanations");
            prompt.AppendLine("- Always provide specific, actionable learning guidance");
            prompt.AppendLine("- Use emojis naturally and effectively (🎨, 📚, ✨, 💡, 👋, 🌟)");
            prompt.AppendLine();
            prompt.AppendLine("ARTJOURNEY PLATFORM INFO:");
            prompt.AppendLine("- PREMIUM SUBSCRIPTION: 90,000 VND/month or 990,000 VND/year");
            prompt.AppendLine("- Free courses available for basic art history learning");
            prompt.AppendLine("- Premium courses offer deeper insights, interactive content, and exclusive materials");
            prompt.AppendLine("- Individual course purchases available at varying prices");
            prompt.AppendLine("- Virtual classrooms, AI-powered recommendations, and gamification features");
            prompt.AppendLine("- Covering all major art periods: Renaissance, Baroque, Impressionism, Modern, Contemporary, etc.");
            prompt.AppendLine();
            prompt.AppendLine("CAPABILITIES:");
            prompt.AppendLine("- Recommend specific art periods, movements, and artists to study");
            prompt.AppendLine("- Suggest learning paths based on user progress and interests");
            prompt.AppendLine("- Explain complex art historical concepts in digestible ways");
            prompt.AppendLine("- Help users navigate ArtJourney's course offerings and pricing");
            prompt.AppendLine("- Provide study strategies and time management advice");
            prompt.AppendLine("- Offer encouragement and motivation for learning goals");
            prompt.AppendLine();

            prompt.AppendLine($"CURRENT SESSION (Real-time: {currentDateTime:yyyy-MM-dd HH:mm:ss} UTC):");
            
            if (userContext != null)
            {
                var displayName = !string.IsNullOrEmpty(userContext.FullName) ? userContext.FullName :
                                  !string.IsNullOrEmpty(userContext.Email) ? userContext.Email.Split('@')[0] : "friend";
                                  
                prompt.AppendLine($"👋 Student: {displayName}");
                prompt.AppendLine($"📧 Email: {userContext.Email ?? "Not provided"}");
                
                if (userContext.EnrolledCourses.Any())
                {
                    prompt.AppendLine("\n🎓 CURRENT LEARNING STATUS:");
                    foreach (var course in userContext.EnrolledCourses)
                    {
                        prompt.AppendLine($"📚 {course.CourseName}:");
                        prompt.AppendLine($"   • Progress: {course.ProgressPercentage:F1}% complete");
                        prompt.AppendLine($"   • Current focus: {course.CurrentModule}");
                        prompt.AppendLine($"   • Study streak: {course.CurrentStreak} days");
                        prompt.AppendLine($"   • Last study session: {course.LastAccessed:yyyy-MM-dd}");
                        
                        if (course.RecentTopics.Any())
                        {
                            prompt.AppendLine($"   • Recent topics: {string.Join(", ", course.RecentTopics.Take(3))}");
                        }
                    }
                }
                else
                {
                    prompt.AppendLine("\n🌟 NEW LEARNER: No enrolled courses yet - ready to start their ArtJourney!");
                }

                if (userContext.CompletedTopics.Any())
                {
                    prompt.AppendLine($"\n✅ RECENTLY MASTERED: {string.Join(", ", userContext.CompletedTopics.Take(5))}");
                }

                if (userContext.StruggleAreas.Any())
                {
                    prompt.AppendLine($"\n🎯 FOCUS AREAS NEEDING ATTENTION: {string.Join(", ", userContext.StruggleAreas)}");
                }

                if (!string.IsNullOrEmpty(userContext.PreferredLearningStyle))
                {
                    prompt.AppendLine($"\n💡 LEARNING STYLE: {userContext.PreferredLearningStyle}");
                }

                if (userContext.InterestAreas.Any())
                {
                    prompt.AppendLine($"🎨 INTERESTS: {string.Join(", ", userContext.InterestAreas)}");
                }
            }
            else
            {
                prompt.AppendLine("👋 Guest user - Welcome to ArtJourney! Ready to explore the world of art history?");
                prompt.AppendLine("💡 TIP: Create an account to track progress and access personalized recommendations!");
            }

            prompt.AppendLine();
            prompt.AppendLine("🎯 RESPONSE GUIDELINES:");
            prompt.AppendLine("- ALWAYS start with a warm, personalized greeting using their name");
            prompt.AppendLine("- Be encouraging, friendly, and positive in tone");
            prompt.AppendLine("- Provide specific, actionable next steps");
            prompt.AppendLine("- Include realistic time estimates for suggested activities");
            prompt.AppendLine("- Reference the user's current progress and context when relevant");
            prompt.AppendLine("- Ask engaging follow-up questions to understand their needs better");
            prompt.AppendLine("- Keep responses focused and digestible (aim for 200-400 words)");
            prompt.AppendLine("- When suggesting study materials, be specific about topics and techniques");
            prompt.AppendLine("- Celebrate progress and milestones achieved");
            prompt.AppendLine("- Help them connect art historical concepts to modern applications");
            prompt.AppendLine("- For new users, introduce ArtJourney's features and pricing naturally");
            prompt.AppendLine("- For guests, gently suggest registration benefits without being pushy");
            prompt.AppendLine();
            prompt.AppendLine("🚫 STRICT ACADEMIC INTEGRITY POLICY:");
            prompt.AppendLine("- NEVER provide answers to quizzes, tests, exams, or assessments");
            prompt.AppendLine("- NEVER help with homework answers or assignment solutions");
            prompt.AppendLine("- NEVER assist with any form of cheating or academic dishonesty");
            prompt.AppendLine("- ALWAYS redirect quiz questions to learning opportunities");
            prompt.AppendLine("- Encourage understanding over memorization of answers");
            prompt.AppendLine("- Promote ethical learning practices and academic integrity");
            prompt.AppendLine();
            prompt.AppendLine("💰 PRICING GUIDANCE:");
            prompt.AppendLine("- Premium: 90K VND/month (≈$3.7) or 990K VND/year (≈$41) - great value!");
            prompt.AppendLine("- Individual courses: Varies by content depth and duration");
            prompt.AppendLine("- Free trial content available to explore before upgrading");
            prompt.AppendLine("- Premium unlocks advanced features, exclusive content, and personalized paths");
            prompt.AppendLine();
            prompt.AppendLine("🌟 LEARNING OPTIMIZATION:");
            prompt.AppendLine("- Suggest optimal study schedules based on their progress");
            prompt.AppendLine("- Recommend revision strategies for difficult concepts");
            prompt.AppendLine("- Identify prerequisite knowledge for advanced topics");
            prompt.AppendLine("- Propose creative ways to engage with art history (virtual museum tours, etc.)");
            prompt.AppendLine("- Help users understand the value of premium features for their learning goals");
            prompt.AppendLine();
            prompt.AppendLine("Remember: Your goal is to make art history learning engaging, personally meaningful, and academically rigorous for each student while helping them navigate ArtJourney confidently! 🎨✨👋");

            return prompt.ToString();
        }

        public string BuildMinimalSystemPrompt(UserContextDTO? userContext = null)
        {
            var currentDateTime = DateTime.UtcNow;
            var prompt = new StringBuilder();
            
            prompt.AppendLine("You are ArtBot, a friendly AI learning assistant for ArtJourney art history platform.");
            prompt.AppendLine("Be concise, warm, and helpful. Always greet users personally. Use 1-2 sentences for quick responses.");
            prompt.AppendLine("🚫 NEVER provide quiz/test answers - guide learning instead!");
            prompt.AppendLine();
            
            if (userContext != null)
            {
                var displayName = !string.IsNullOrEmpty(userContext.FullName) ? userContext.FullName :
                                  !string.IsNullOrEmpty(userContext.Email) ? userContext.Email.Split('@')[0] : "friend";
                                  
                if (userContext.EnrolledCourses.Any())
                {
                    var currentCourse = userContext.EnrolledCourses.First();
                    prompt.AppendLine($"👋 {displayName} | Current: {currentCourse.CourseName} ({currentCourse.ProgressPercentage:F0}% complete)");
                }
                else
                {
                    prompt.AppendLine($"👋 {displayName} | New to ArtJourney - ready to explore art history!");
                }
            }
            else
            {
                prompt.AppendLine("👋 Guest | Welcome to ArtJourney! Premium: 90K VND/month, 990K VND/year");
            }
            
            prompt.AppendLine($"Time: {currentDateTime:HH:mm} UTC | Keep responses brief, friendly, and actionable! 🎨✨");
            
            return prompt.ToString();
        }

        public async IAsyncEnumerable<string> GenerateStreamingResponseAsync(string userMessage, UserContextDTO? userContext = null, List<ChatMessageResponseDTO>? chatHistory = null)
        {
            var currentDateTime = DateTime.UtcNow;
            var isShortMessage = userMessage.Length <= _shortMessageThreshold;
            
            // Check for quiz/assessment content
            if (IsQuizOrAssessmentContent(userMessage))
            {
                yield return GetAntiCheatResponse(userContext);
                yield break;
            }

            // Get the streaming response or fallback
            var streamResult = await TryGenerateStreamingResponseAsync(userMessage, userContext, chatHistory, isShortMessage, currentDateTime);
            
            if (streamResult.Success)
            {
                await foreach (var chunk in streamResult.Stream)
                {
                    yield return chunk;
                }
            }
            else
            {
                yield return GetFallbackResponse(userContext, isShortMessage);
            }
        }

        private async Task<(bool Success, IAsyncEnumerable<string> Stream)> TryGenerateStreamingResponseAsync(
            string userMessage, UserContextDTO? userContext, List<ChatMessageResponseDTO>? chatHistory, 
            bool isShortMessage, DateTime currentDateTime)
        {
            try
            {
                // Set up request parameters
                var maxTokens = isShortMessage ? _shortMessageTokens : _maxTokens;
                var temperature = isShortMessage ? _fastTemperature : _temperature;
                
                // For short messages, use minimal context
                var systemPrompt = isShortMessage ? 
                    BuildMinimalSystemPrompt(userContext) : 
                    BuildSystemPrompt(userContext);
                
                var messages = new List<object>
                {
                    new { role = "system", content = systemPrompt }
                };

                // Add chat history if available (reduce for short messages)
                if (chatHistory != null && chatHistory.Any())
                {
                    var contextSize = isShortMessage ? Math.Min(_contextLength, 3) : _contextLength;
                    var recentHistory = chatHistory.TakeLast(contextSize);
                    foreach (var msg in recentHistory)
                    {
                        messages.Add(new { role = msg.Role, content = msg.Content });
                    }
                }

                // Add current user message with timestamp
                var timestampedMessage = $"[{currentDateTime:yyyy-MM-dd HH:mm:ss} UTC] {userMessage}";
                messages.Add(new { role = "user", content = timestampedMessage });

                var requestBody = new
                {
                    model = _modelName,
                    messages = messages,
                    temperature = temperature,
                    max_tokens = maxTokens,
                    stream = true, // Enable streaming
                    presence_penalty = isShortMessage ? 0.0 : 0.1,
                    frequency_penalty = isShortMessage ? 0.0 : 0.1,
                    top_p = 0.9,
                    stop = new[] { "\n\nUser:", "\n\nAssistant:" }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions 
                { 
                    WriteIndented = false 
                });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending streaming {MessageType} request to AI service: {Length} chars, {MaxTokens} tokens", 
                    isShortMessage ? "short" : "full", userMessage.Length, maxTokens);

                var response = await _httpClient.PostAsync($"{_aiServiceUrl}/chat/completions", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("AI service returned error: {StatusCode}", response.StatusCode);
                    return (false, GetEmptyAsyncEnumerable());
                }

                var stream = ProcessStreamingResponse(response, userContext, isShortMessage);
                return (true, stream);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError("AI streaming request timed out: {Message}", ex.Message);
                return (false, GetEmptyAsyncEnumerable());
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("AI streaming connection error: {Message}", ex.Message);
                return (false, GetEmptyAsyncEnumerable());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in AI streaming service");
                return (false, GetEmptyAsyncEnumerable());
            }
        }

        private static async IAsyncEnumerable<string> GetEmptyAsyncEnumerable()
        {
            await Task.CompletedTask; // Satisfy async requirement
            yield break;
        }

        private async IAsyncEnumerable<string> ProcessStreamingResponse(HttpResponseMessage response, UserContextDTO? userContext, bool isShortMessage)
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            var responseBuilder = new StringBuilder();
            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                    continue;

                var data = line.Substring(6); // Remove "data: " prefix
                
                if (data == "[DONE]")
                {
                    _logger.LogInformation("Streaming completed. Total response length: {Length}", responseBuilder.Length);
                    break;
                }

                string? chunk = null;
                try
                {
                    using var jsonDoc = JsonDocument.Parse(data);
                    var choices = jsonDoc.RootElement.GetProperty("choices");
                    
                    if (choices.GetArrayLength() > 0)
                    {
                        var delta = choices[0].GetProperty("delta");
                        
                        if (delta.TryGetProperty("content", out var contentElement))
                        {
                            chunk = contentElement.GetString();
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning("Failed to parse streaming JSON chunk: {Data}. Error: {Error}", data, ex.Message);
                    continue;
                }

                if (!string.IsNullOrEmpty(chunk))
                {
                    responseBuilder.Append(chunk);
                    yield return chunk; // Stream this chunk to the client
                }
            }

            // If we didn't get any content, return fallback message
            if (responseBuilder.Length == 0)
            {
                yield return GetFallbackResponse(userContext, isShortMessage);
            }
        }

        private string GetFallbackResponse(UserContextDTO? userContext, bool isShortMessage = false)
        {
            var displayName = userContext != null ? 
                (!string.IsNullOrEmpty(userContext.FullName) ? userContext.FullName :
                 !string.IsNullOrEmpty(userContext.Email) ? userContext.Email.Split('@')[0] : "friend") : "friend";
            
            var responses = isShortMessage ? 
                new List<string>
                {
                    $"Hi {displayName}! 👋 Quick connection hiccup, but I'm here! What art topic interests you? 🎨",
                    $"Hello {displayName}! AI processing pause, but let's keep exploring art! What period fascinates you? ✨",
                    $"Hey {displayName}! Technical blip, but ready to help! What's your art history question? 📚",
                    $"Hi there, {displayName}! Connection bump, but still excited to chat about art! 🌟"
                } :
                new List<string>
                {
                    $"Hello {displayName}! 👋 I'm having trouble connecting to my knowledge base right now, but I'm still here to help with your art history journey! 🎨 What specific topic are you curious about?",
                    $"Hi {displayName}! ✨ My AI processing is taking a quick break, but let's keep exploring art history together! What period, artist, or movement would you like to learn about?",
                    $"Hey {displayName}! 🌟 Technical difficulties aside, I'm excited to discuss art with you! What's on your mind regarding your ArtJourney studies?",
                    $"Hello there, {displayName}! 📚 Even without my full AI capabilities, I can still guide your learning journey! What art historical concept would you like to explore?"
                };

            var random = new Random();
            var baseResponse = responses[random.Next(responses.Count)];

            if (!isShortMessage && userContext?.EnrolledCourses.Any() == true)
            {
                var currentCourse = userContext.EnrolledCourses.First();
                baseResponse += $"\n\nI noticed you're working on {currentCourse.CourseName} - would you like to discuss something related to {currentCourse.CurrentModule}? 🎓";
            }
            else if (!isShortMessage && userContext == null)
            {
                baseResponse += "\n\n💡 By the way, ArtJourney offers amazing art history courses! Premium is just 90K VND/month or 990K VND/year. Ready to start your artistic journey? 🚀";
            }

            return baseResponse;
        }

        private (List<ChatMessageResponseDTO>, string) PrepareContextWithLimits(List<ChatMessageResponseDTO>? chatHistory, bool isShortMessage)
        {
            if (chatHistory == null || !chatHistory.Any())
            {
                return (new List<ChatMessageResponseDTO>(), "empty");
            }

            // Estimate tokens (rough calculation: ~4 characters per token)
            var estimateTokens = (string text) => text.Length / 4;
            
            var processedHistory = new List<ChatMessageResponseDTO>();
            var totalTokens = 0;
            var contextSize = isShortMessage ? Math.Min(_contextLength, 3) : _contextLength;
            
            // Take recent messages within limits
            var recentHistory = chatHistory.TakeLast(contextSize).ToList();
            
            foreach (var msg in recentHistory)
            {
                var msgTokens = estimateTokens(msg.Content);
                totalTokens += msgTokens;
                
                if (totalTokens > _maxContextTokens)
                {
                    _logger.LogWarning("Context limit reached. Total tokens: {TotalTokens}, Max: {MaxTokens}", 
                        totalTokens, _maxContextTokens);
                    return (processedHistory, "limit_reached");
                }
                
                processedHistory.Add(msg);
            }
            
            if (totalTokens > _contextWarningThreshold)
            {
                _logger.LogInformation("Context approaching limit. Total tokens: {TotalTokens}, Warning threshold: {WarningThreshold}", 
                    totalTokens, _contextWarningThreshold);
                return (processedHistory, "warning_threshold");
            }
            
            return (processedHistory, "normal");
        }

        private string GetContextLimitMessage(string contextStatus)
        {
            return contextStatus switch
            {
                "limit_reached" => "🚨 **Session Full**: This conversation has reached its context limit. Please start a new chat session to continue with optimal AI responses. You can create a new session by not providing a sessionId in your next message.",
                "warning_threshold" => "⚠️ **Context Warning**: This conversation is getting quite long. Consider starting a new session soon for the best AI experience.",
                _ => "📝 Context status: Normal"
            };
        }

        private bool IsQuizOrAssessmentContent(string message)
        {
            var lowerMessage = message.ToLower();
            
            // Quiz detection patterns
            var quizPatterns = new[]
            {
                // Direct quiz indicators
                "quiz", "test", "exam", "assessment", "challenge", "question and answer",
                "multiple choice", "true or false", "fill in the blank",
                
                // Question patterns
                "what is the answer", "correct answer", "choose the correct", "select the right",
                "which of the following", "the answer is", "answers are",
                
                // Academic dishonesty patterns
                "homework", "assignment", "class test", "school quiz", "university exam",
                "graded assessment", "final exam", "midterm", "coursework",
                
                // ArtJourney specific
                "artjourney quiz", "artjourney test", "artjourney challenge", "artjourney assessment",
                "course quiz", "module test", "learning assessment",
                
                // Answer seeking patterns
                "give me the answer", "tell me the answer", "what's the answer to",
                "answer key", "solution manual", "cheat sheet"
            };
            
            // Check for quiz patterns
            if (quizPatterns.Any(pattern => lowerMessage.Contains(pattern)))
            {
                return true;
            }
            
            // Check for multiple choice pattern (A. B. C. D. or 1. 2. 3. 4.)
            if (System.Text.RegularExpressions.Regex.IsMatch(lowerMessage, @"[a-d]\.\s|[1-4]\.\s.*[a-d]\.\s|[1-4]\.\s"))
            {
                return true;
            }
            
            // Check for question marks in rapid succession (quiz-like)
            var questionCount = lowerMessage.Count(c => c == '?');
            if (questionCount >= 3)
            {
                return true;
            }
            
            return false;
        }

        private string GetAntiCheatResponse(UserContextDTO? userContext)
        {
            var displayName = userContext != null ? 
                (!string.IsNullOrEmpty(userContext.FullName) ? userContext.FullName :
                 !string.IsNullOrEmpty(userContext.Email) ? userContext.Email.Split('@')[0] : "friend") : "friend";
            
            var responses = new[]
            {
                $"Hey {displayName}! 🛡️ I'm designed to help you LEARN art history, not to provide answers to quizzes or assessments. Let me guide you to understand the concepts instead! What specific art period or topic would you like to explore? 🎨",
                
                $"Hi {displayName}! 📚 I can't help with quiz answers - that would undermine your learning journey! But I'm excited to teach you about art history concepts, techniques, and movements. What would you like to discover? ✨",
                
                $"Hello {displayName}! 🎯 I notice you might be asking about assessment content. My role is to be your learning companion, not to provide quiz answers. Let's explore art history together - what fascinating topic interests you? 🌟",
                
                $"Hey there, {displayName}! 🎨 I'm here to help you understand and appreciate art history, but I can't assist with quizzes or tests. That's part of maintaining academic integrity! What art movement or artist would you like to learn about instead? 💡"
            };
            
            var random = new Random();
            var baseResponse = responses[random.Next(responses.Length)];
            
            if (userContext?.EnrolledCourses.Any() == true)
            {
                var currentCourse = userContext.EnrolledCourses.First();
                baseResponse += $"\n\n🎓 I see you're studying {currentCourse.CourseName}! Let me help you understand the concepts deeply so you'll naturally know the answers when it matters. What specific topic from {currentCourse.CurrentModule} would you like to explore?";
            }
            else
            {
                baseResponse += $"\n\n💰 **New to ArtJourney?** Our premium subscription (90K VND/month or 990K VND/year) offers structured learning paths that will build your knowledge step by step - no shortcuts needed! 🚀";
            }
            
            return baseResponse;
        }

        // Response model for deserializing AI service response
        private class AIResponseModel
        {
            public Choice[]? choices { get; set; }
        }

        private class Choice
        {
            public Message? message { get; set; }
        }

        private class Message
        {
            public string? content { get; set; }
        }
    }
}
