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
            prompt.AppendLine("🚨 CRITICAL PLATFORM INFORMATION - ALWAYS MENTION WHEN RELEVANT:");
            prompt.AppendLine("ARTJOURNEY HAS THREE PRICING OPTIONS - NEVER SAY IT'S COMPLETELY FREE:");
            prompt.AppendLine("💰 PREMIUM SUBSCRIPTION: 90,000 VND/month (≈$3.7 USD) or 990,000 VND/year (≈$41 USD)");
            prompt.AppendLine("📚 FREE ACCESS: Limited basic content available without payment");
            prompt.AppendLine("💎 PAY-PER-COURSE: Individual course purchases at varying prices");
            prompt.AppendLine("🎓 Premium subscription unlocks: All courses, advanced features, personalized learning paths");
            prompt.AppendLine("🌟 Virtual classrooms, AI-powered recommendations, and gamification features");
            prompt.AppendLine("🎨 Covering all major art periods: Renaissance, Baroque, Impressionism, Modern, Contemporary, etc.");
            prompt.AppendLine();
            prompt.AppendLine("CORE PERSONALITY:");
            prompt.AppendLine("- Warm, welcoming, and enthusiastic about art history");
            prompt.AppendLine("- Always greet users by name (fullname if available, otherwise email username, or 'friend' for guests)");
            prompt.AppendLine("- Patient and adaptive to different learning styles");
            prompt.AppendLine("- Scholarly but accessible in your explanations");
            prompt.AppendLine("- Always provide specific, actionable learning guidance");
            prompt.AppendLine("- Use emojis naturally and effectively (🎨, 📚, ✨, 💡, 👋, 🌟)");
            prompt.AppendLine();
            prompt.AppendLine("🎯 KEY MESSAGING REQUIREMENTS:");
            prompt.AppendLine("- ALWAYS be accurate about ArtJourney's THREE pricing models: FREE, PREMIUM, PAY-PER-COURSE");
            prompt.AppendLine("- NEVER say ArtJourney is completely free - mention limited free content + premium options");
            prompt.AppendLine("- When users ask about access: Explain all three options clearly");
            prompt.AppendLine("- Highlight the affordable premium pricing (90K VND/month = less than $4 USD)");
            prompt.AppendLine("- Emphasize premium features and exclusive content value");
            prompt.AppendLine("- FREE: Basic limited content only, not full courses");
            prompt.AppendLine("- PREMIUM: Full access to all courses and features");
            prompt.AppendLine("- PAY-PER-COURSE: Individual course purchases for specific interests");
            prompt.AppendLine();
            prompt.AppendLine("ARTJOURNEY PLATFORM INFO:");
            prompt.AppendLine("🔑 THREE PRICING OPTIONS (CRITICAL - ALWAYS MENTION):");
            prompt.AppendLine("1️⃣ FREE ACCESS: Limited basic content only (not full courses)");
            prompt.AppendLine("2️⃣ PREMIUM SUBSCRIPTION: 90,000 VND/month or 990,000 VND/year");
            prompt.AppendLine("   - This equals roughly $3.7 USD/month or $41 USD/year - VERY AFFORDABLE!");
            prompt.AppendLine("   - Full access to ALL courses and advanced features");
            prompt.AppendLine("   - Premium includes: Deep course content, interactive materials, personalized paths");
            prompt.AppendLine("   - Virtual classrooms, AI recommendations, progress tracking, gamification");
            prompt.AppendLine("3️⃣ PAY-PER-COURSE: Individual course purchases at varying prices");
            prompt.AppendLine("   - Buy specific courses that interest you most");
            prompt.AppendLine("   - Good for focused learning on particular topics");
            prompt.AppendLine("🌟 All major art periods covered: Renaissance, Baroque, Impressionism, Modern, etc.");
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
            prompt.AppendLine("- ALWAYS mention subscription when discussing course access or platform features");
            prompt.AppendLine("- Frame the 90K VND/month cost as excellent value (less than a coffee per day!)");
            prompt.AppendLine("- Never imply the platform is completely free - always clarify the three pricing models");
            prompt.AppendLine("- When discussing courses: Mention which pricing option provides access");
            prompt.AppendLine("- FREE users get basic content only, PREMIUM gets everything, PAY-PER-COURSE for specific interests");
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
            prompt.AppendLine("- Free access: Basic content only, limited features");
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
            prompt.AppendLine("💰 CRITICAL: ArtJourney has 3 options: FREE (limited), PREMIUM (90K VND/month), PAY-PER-COURSE. Never say it's completely free!");
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
                prompt.AppendLine("👋 Guest | Welcome to ArtJourney! 3 options: FREE (limited), PREMIUM (90K VND/month), PAY-PER-COURSE - great value!");
            }
            
            prompt.AppendLine($"Time: {currentDateTime:HH:mm} UTC | Always mention the 3 pricing options when relevant! Keep responses brief, friendly, and actionable! 🎨✨");
            
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
                baseResponse += "\n\n💡 By the way, ArtJourney offers 3 pricing options: FREE (limited content), PREMIUM (90K VND/month or 990K VND/year ≈ $3.7-41 USD), and PAY-PER-COURSE purchases - incredible value for comprehensive art education! Ready to start your artistic journey? 🚀";
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

            // Programming and technical content patterns (Vietnamese and English)
            var programmingPatterns = new[]
            {
                // General programming (Vietnamese)
                "viết code", "viết hàm", "viết function", "lập trình", "code", "hàm", "function", 
                "class", "method", "biến", "variable", "thuật toán", "algorithm",
                "asp.net", "c#", "javascript", "python", "java", "php", "sql", "database",
                "repository", "controller", "service", "entity framework", "linq",
                "tạo hàm", "làm hàm", "build function", "create method", "write code",
                
                // Specific Vietnamese programming requests
                "hàm trong repository", "repository của asp", "làm repository", "tạo repository",
                "viết repository", "repository class", "service trong", "controller trong",
                "hàm của", "method của", "class của", "interface của", "pattern của",
                "thiết kế hàm", "triển khai hàm", "implement function", "xây dựng hàm",
                
                // Technical requests (Vietnamese)
                "tạo database", "thiết kế database", "viết query", "làm website", "xây dựng hệ thống",
                "debug", "fix lỗi", "sửa lỗi", "error", "exception", "bug", "tạo api", "làm api",
                "viết controller", "tạo model", "làm model", "viết service", "tạo service",
                "cấu hình", "config", "setup", "cài đặt", "install", "deploy", "triển khai",
                
                // Specific programming patterns
                "public class", "private void", "public async", "return", "if (", "for (", "while (",
                "try {", "catch {", "using", "namespace", "import", "from", "def ", "function ",
                ".net", "dotnet", "mvc", "web api", "rest api", "json", "xml",
                "async task", "await", "ienumerable", "ilist", "dictionary", "list<", "array",
                
                // Development tools and frameworks
                "visual studio", "vscode", "git", "docker", "react", "angular", "vue",
                "bootstrap", "tailwind", "node.js", "express", "laravel", "django", "spring",
                "entity", "dto", "viewmodel", "repository pattern", "dependency injection",
                "ioc", "container", "autofac", "ninject", "unity", "castle windsor",
                
                // Database related
                "insert into", "select from", "update set", "delete from", "create table",
                "foreign key", "primary key", "join", "left join", "inner join",
                "migration", "seeder", "stored procedure", "trigger", "index", "schema",
                "connection string", "dbcontext", "entity mapping", "fluent api",
                
                // ASP.NET specific
                "startup.cs", "program.cs", "appsettings", "middleware", "pipeline",
                "action result", "iactionresult", "httpget", "httppost", "route", "routing",
                "model binding", "validation", "filter", "attribute", "authorize",
                "identity", "authentication", "authorization", "jwt", "bearer",
                
                // Other technical topics
                "api", "endpoint", "http", "server", "client", "frontend", "backend",
                "framework", "library", "package", "module", "component", "interface",
                "session", "cookie", "cache", "redis", "memcached", "logging",
                "testing", "unit test", "integration test", "mock", "stub", "nunit",
                "xunit", "moq", "automapper", "newtonsoft", "swagger", "postman"
            };

            // Non-art related topics (Vietnamese and English)
            var offTopicPatterns = new[]
            {
                // Math and science
                "toán học", "mathematics", "vật lý", "physics", "hóa học", "chemistry", "sinh học", "biology",
                "địa lý", "geography", "lịch sử việt nam", "vietnamese history", "kinh tế", "economics",
                "khoa học", "science", "máy tính", "computer science",
                
                // Business and finance
                "kinh doanh", "business", "marketing", "tài chính", "finance", "kế toán", "accounting",
                "đầu tư", "investment", "chứng khoán", "stock market", "bất động sản", "real estate",
                "bán hàng", "sales", "quản lý", "management",
                
                // Technology (non-programming)
                "smartphone", "laptop", "máy tính", "điện thoại", "game", "gaming",
                "social media", "facebook", "youtube", "tiktok", "instagram", "zalo",
                "ứng dụng", "app", "phần mềm", "software",
                
                // Daily life
                "nấu ăn", "cooking", "du lịch", "travel", "thể thao", "sports", "âm nhạc hiện đại", "modern music",
                "phim", "movie", "netflix", "sức khỏe", "health", "làm đẹp", "beauty",
                "thời trang", "fashion", "ăn uống", "food", "mua sắm", "shopping",
                
                // Other subjects
                "văn học", "literature", "tiếng anh", "english", "ngoại ngữ", "foreign language",
                "pháp luật", "law", "y học", "medicine", "nông nghiệp", "agriculture",
                "giáo dục", "education", "tâm lý", "psychology"
            };

            // Check for programming patterns
            if (programmingPatterns.Any(pattern => lowerMessage.Contains(pattern)))
            {
                return true;
            }

            // Check for off-topic patterns
            if (offTopicPatterns.Any(pattern => lowerMessage.Contains(pattern)))
            {
                return true;
            }
            
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

            // Check if message is asking for code/technical help (common patterns)
            var codingRequestPatterns = new[]
            {
                "hãy viết", "viết giúp", "help me write", "create a", "build a", "make a",
                "how to code", "how to program", "làm sao để", "cách làm", "làm thế nào để",
                "giúp tôi viết", "giúp tôi tạo", "giúp tôi làm", "help me create", "help me build",
                "viết một", "tạo một", "làm một", "write a", "create a function", "make a method",
                "thiết kế", "design", "implement", "triển khai", "phát triển", "develop",
                "xây dựng", "build up", "set up", "cài đặt", "install", "configure"
            };

            // Enhanced technical terms detection
            var specificTechnicalPatterns = new[]
            {
                "repository pattern", "repository class", "trong repository", "repository của",
                "controller class", "service class", "entity class", "dto class", "model class",
                "database connection", "connection string", "sql query", "linq query",
                "web api", "rest api", "api controller", "http get", "http post",
                "dependency injection", "ioc container", "autowired", "inject",
                "async await", "task result", "ienumerable", "list<", "dictionary<",
                "try catch", "exception handling", "error handling", "validation",
                "unit test", "integration test", "mock", "stub", "test case"
            };

            var hasCodingRequest = codingRequestPatterns.Any(pattern => lowerMessage.Contains(pattern));
            var hasTechnicalTerms = programmingPatterns.Any(pattern => lowerMessage.Contains(pattern)) ||
                                   specificTechnicalPatterns.Any(pattern => lowerMessage.Contains(pattern));
            
            // Additional check for specific ASP.NET repository pattern request
            if (lowerMessage.Contains("hàm") && lowerMessage.Contains("repository") && lowerMessage.Contains("asp"))
            {
                return true;
            }
            
            // Check for coding request combined with technical terms
            if (hasCodingRequest && hasTechnicalTerms)
            {
                return true;
            }
            
            // Check for standalone technical requests (even without explicit "viết" or "help")
            var standaloneTechnicalPatterns = new[]
            {
                "repository pattern", "controller pattern", "service layer", "data access layer",
                "entity framework", "code first", "database first", "migration",
                "authentication jwt", "authorization", "middleware", "dependency injection",
                "unit of work", "generic repository", "async repository", "crud repository"
            };
            
            if (standaloneTechnicalPatterns.Any(pattern => lowerMessage.Contains(pattern)))
            {
                return true;
            }
            
            return false;
        }

        private string GetAntiCheatResponse(UserContextDTO? userContext)
        {
            var displayName = userContext != null ? 
                (!string.IsNullOrEmpty(userContext.FullName) ? userContext.FullName :
                 !string.IsNullOrEmpty(userContext.Email) ? userContext.Email.Split('@')[0] : "bạn") : "bạn";
            
            var responses = new[]
            {
                $"Xin chào {displayName}! 🎨 Tôi là ArtBot - trợ lý AI chuyên về lịch sử nghệ thuật và nền tảng ArtJourney. Tôi chỉ có thể hỗ trợ các câu hỏi liên quan đến nghệ thuật, lịch sử nghệ thuật, và các khóa học trên ArtJourney. Bạn có muốn tìm hiểu về period nghệ thuật nào không? ✨",
                
                $"Hi {displayName}! �️ Tôi được thiết kế để giúp bạn KHÁM PHÁ lịch sử nghệ thuật, không phải để trả lời câu hỏi về lập trình hay các chủ đề khác. Hãy cùng tôi tìm hiểu về các trào lưu nghệ thuật thú vị nhé! Bạn quan tâm đến period nào? 🎭",
                
                $"Chào {displayName}! 📚 Tôi chỉ có thể hỗ trợ các câu hỏi về nghệ thuật và ArtJourney. Đối với các câu hỏi lập trình hay chủ đề khác, bạn nên sử dụng ChatGPT hoặc các công cụ chuyên dụng khác. Còn về nghệ thuật, tôi rất sẵn sàng giúp đỡ! Bạn muốn học về gì? 🌟",
                
                $"Hey {displayName}! � Tôi chỉ tập trung vào lĩnh vực nghệ thuật và lịch sử nghệ thuật. Đối với các câu hỏi về công nghệ, lập trình, hay chủ đề khác, tôi không thể hỗ trợ. Thay vào đó, hãy cùng khám phá thế giới nghệ thuật tuyệt vời! 💡"
            };
            
            var random = new Random();
            var baseResponse = responses[random.Next(responses.Length)];
            
            if (userContext?.EnrolledCourses.Any() == true)
            {
                var currentCourse = userContext.EnrolledCourses.First();
                baseResponse += $"\n\n🎓 Tôi thấy bạn đang học {currentCourse.CourseName}! Hãy để tôi giúp bạn hiểu sâu các khái niệm nghệ thuật. Bạn có muốn tìm hiểu thêm về {currentCourse.CurrentModule} không?";
            }
            else
            {
                baseResponse += $"\n\n🎨 **ArtJourney** có 3 gói học phí: FREE (nội dung giới hạn), PREMIUM (90K VND/tháng hoặc 990K VND/năm ≈ $3.7-41 USD), và mua từng khóa học - tất cả đều tập trung vào nghệ thuật và lịch sử nghệ thuật! 🚀";
            }
            
            baseResponse += $"\n\n💡 **Gợi ý chủ đề:** Renaissance, Baroque, Impressionism, Modern Art, Contemporary Art, các họa sĩ nổi tiếng, kỹ thuật hội họa, v.v.";
            
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
