using BusinessObjects.Models;
using DAOs;
using Helpers.DTOs.Chat;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Queries;

namespace Repositories.Implements
{
    public class ChatRepository : IChatRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public ChatRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<ChatSession> CreateChatSessionAsync(ChatSession chatSession)
        {
            var createdSession = await _unitOfWork.GetRepo<ChatSession>().CreateAsync(chatSession);
            await _unitOfWork.SaveChangesAsync();
            return createdSession;
        }

        public async Task<ChatMessage> CreateChatMessageAsync(ChatMessage chatMessage)
        {
            var createdMessage = await _unitOfWork.GetRepo<ChatMessage>().CreateAsync(chatMessage);
            await _unitOfWork.SaveChangesAsync();
            return createdMessage;
        }

        public async Task<ChatSession?> GetChatSessionByIdAsync(long chatSessionId, long userId)
        {
            var queryOptions = new QueryBuilder<ChatSession>()
                .WithTracking(false)
                .WithPredicate(cs => cs.ChatSessionId == chatSessionId && cs.UserId == userId)
                .WithInclude(cs => cs.ChatMessages)
                .Build();
            return await _unitOfWork.GetRepo<ChatSession>().GetSingleAsync(queryOptions);
        }

        public async Task<List<ChatSession>> GetUserChatSessionsAsync(long userId, int pageNumber = 1, int pageSize = 10)
        {
            var queryOptions = new QueryBuilder<ChatSession>()
                .WithTracking(false)
                .WithPredicate(cs => cs.UserId == userId && cs.IsActive)
                .WithOrderBy(query => query.OrderByDescending(cs => cs.UpdatedAt))
                .Build();
            
            var allSessions = await _unitOfWork.GetRepo<ChatSession>().GetAllAsync(queryOptions);
            return allSessions.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }

        public async Task<List<ChatMessage>> GetChatMessagesAsync(long chatSessionId, long userId)
        {
            // First verify user owns the session
            var session = await GetChatSessionByIdAsync(chatSessionId, userId);
            if (session == null) return new List<ChatMessage>();

            var queryOptions = new QueryBuilder<ChatMessage>()
                .WithTracking(false)
                .WithPredicate(cm => cm.ChatSessionId == chatSessionId)
                .WithOrderBy(query => query.OrderBy(cm => cm.Timestamp))
                .Build();
            return (await _unitOfWork.GetRepo<ChatMessage>().GetAllAsync(queryOptions)).ToList();
        }

        public async Task<bool> UpdateChatSessionAsync(ChatSession chatSession)
        {
            try
            {
                await _unitOfWork.GetRepo<ChatSession>().UpdateAsync(chatSession);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeactivateChatSessionAsync(long chatSessionId, long userId)
        {
            try
            {
                var session = await GetChatSessionByIdAsync(chatSessionId, userId);
                if (session == null) return false;

                session.IsActive = false;
                session.UpdatedAt = DateTime.UtcNow;
                
                await _unitOfWork.GetRepo<ChatSession>().UpdateAsync(session);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserContextDTO> GetUserContextAsync(long userId)
        {
            try
            {
                var userContext = new UserContextDTO
                {
                    UserId = userId,
                    CurrentDateTime = DateTime.UtcNow,
                    TimeZone = "UTC"
                };

                // Get user basic info
                var user = await _context.Users
                    .Where(u => u.UserId == userId)
                    .Select(u => new { u.Fullname, u.Email })
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    userContext.FullName = user.Fullname ?? "";
                    userContext.Email = user.Email ?? "";
                }
                else
                {
                    // If user not found, return empty context
                    userContext.FullName = "";
                    userContext.Email = "";
                    userContext.EnrolledCourses = new List<CourseProgressDTO>();
                    userContext.CompletedTopics = new List<string>();
                    userContext.StruggleAreas = new List<string>();
                    return userContext;
                }

                // Get enrolled courses with basic data first
                var enrolledCoursesQuery = from uci in _context.UserCourseInfos
                                         where uci.UserId == userId && uci.Course != null
                                         select new
                                         {
                                             uci.CourseId,
                                             CourseName = uci.Course!.Title ?? "",
                                             ProgressPercentage = uci.ProgressPercent,
                                             uci.EnrolledAt,
                                             CourseDescription = uci.Course.Description ?? ""
                                         };

                var enrolledCourses = await enrolledCoursesQuery.ToListAsync();

                // If no enrolled courses, return empty lists
                if (!enrolledCourses.Any())
                {
                    userContext.EnrolledCourses = new List<CourseProgressDTO>();
                    userContext.CompletedTopics = new List<string>();
                    userContext.StruggleAreas = new List<string>();
                    return userContext;
                }

                // Get streak info for all courses
                var streakInfos = await _context.userCourseStreaks
                    .Where(ucs => ucs.UserId == userId)
                    .ToDictionaryAsync(ucs => ucs.CourseId, ucs => new { ucs.CurrentStreak, ucs.LastAccessDate });

                userContext.EnrolledCourses = enrolledCourses.Select(ec => new CourseProgressDTO
                {
                    CourseId = ec.CourseId,
                    CourseName = ec.CourseName,
                    ProgressPercentage = (double)ec.ProgressPercentage,
                    LastAccessed = ec.EnrolledAt ?? DateTime.UtcNow,
                    CurrentStreak = streakInfos.ContainsKey(ec.CourseId) ? streakInfos[ec.CourseId].CurrentStreak : 0,
                    CurrentModule = "Getting Started", // Will be populated below
                    RecentTopics = new List<string>() // Will be populated below
                }).ToList();

            // Get recent learning activities for each course
            foreach (var course in userContext.EnrolledCourses)
            {
                // Get current module (first incomplete module)
                var currentModule = await (from umi in _context.UserModuleInfos
                                         join m in _context.Modules on umi.ModuleId equals m.ModuleId
                                         where umi.UserId == userId && 
                                               m.CourseId == course.CourseId && 
                                               !umi.IsCompleted
                                         orderby m.ModuleId
                                         select m.ModuleTitle).FirstOrDefaultAsync();
                
                course.CurrentModule = currentModule ?? "Getting Started";

                var recentTopics = await _context.UserLearningProgresses
                    .Where(ulp => ulp.UserId == userId && 
                                 ulp.LearningContent!.CourseId == course.CourseId &&
                                 ulp.Status == BusinessObjects.Enums.UserLearningProgressStatus.Completed)
                    .OrderByDescending(ulp => ulp.UpdatedAt)
                    .Take(3)
                    .Select(ulp => ulp.LearningContent!.Title ?? "")
                    .ToListAsync();

                course.RecentTopics = recentTopics;
            }

            // Get completed topics (recent 10 completed learning content)
            var completedTopics = await _context.UserLearningProgresses
                .Where(ulp => ulp.UserId == userId && 
                             ulp.Status == BusinessObjects.Enums.UserLearningProgressStatus.Completed)
                .Include(ulp => ulp.LearningContent)
                .OrderByDescending(ulp => ulp.UpdatedAt)
                .Take(10)
                .Select(ulp => ulp.LearningContent!.Title ?? "")
                .ToListAsync();

            userContext.CompletedTopics = completedTopics;

            // Identify struggle areas (learning content with multiple attempts or low scores)
            var struggleAreas = await _context.UserLearningProgresses
                .Where(ulp => ulp.UserId == userId && 
                             (ulp.Attempts > 2 || ulp.Score < 70) &&
                             ulp.LearningContent != null)
                .Include(ulp => ulp.LearningContent)
                .Select(ulp => ulp.LearningContent!.Title ?? "")
                .Distinct()
                .Take(5)
                .ToListAsync();

            userContext.StruggleAreas = struggleAreas;

            // Get interest areas based on course themes and regions
            var interestAreas = await _context.UserCourseInfos
                .Where(uci => uci.UserId == userId)
                .Include(uci => uci.Course)
                .ThenInclude(c => c!.CourseRegion)
                .Include(uci => uci.Course)
                .ThenInclude(c => c!.CourseHistoricalPeriod)
                .Select(uci => new 
                { 
                    Region = uci.Course!.CourseRegion!.RegionName,
                    Period = uci.Course.CourseHistoricalPeriod!.HistoricalPeriodName
                })
                .ToListAsync();

            var interests = new List<string>();
            interests.AddRange(interestAreas.Where(ia => !string.IsNullOrEmpty(ia.Region)).Select(ia => ia.Region).Distinct());
            interests.AddRange(interestAreas.Where(ia => !string.IsNullOrEmpty(ia.Period)).Select(ia => ia.Period).Distinct());
            
            userContext.InterestAreas = interests.Distinct().ToList();

            // Set preferred learning style based on user activity patterns
            var learningPatterns = await _context.UserLearningProgresses
                .Where(ulp => ulp.UserId == userId && ulp.CompletedIn.HasValue)
                .GroupBy(ulp => 1)
                .Select(g => new 
                {
                    AvgCompletionTime = g.Average(ulp => ulp.CompletedIn!.Value.TotalMinutes),
                    TotalCompleted = g.Count()
                })
                .FirstOrDefaultAsync();

            if (learningPatterns != null)
            {
                if (learningPatterns.AvgCompletionTime < 10)
                    userContext.PreferredLearningStyle = "Quick Learner - Prefers concise content";
                else if (learningPatterns.AvgCompletionTime > 30)
                    userContext.PreferredLearningStyle = "Deep Learner - Enjoys comprehensive study";
                else
                    userContext.PreferredLearningStyle = "Balanced Learner - Adapts to content type";
            }

            return userContext;
            }
            catch (Exception ex)
            {
                // Log the error and return empty context
                throw new InvalidOperationException($"Error retrieving user context for user {userId}: {ex.Message}", ex);
            }
        }

        public async Task<UserLearningAnalyticsDTO> GetLearningAnalyticsAsync(long userId)
        {
            try
            {
                var analytics = new UserLearningAnalyticsDTO
                {
                    UserId = userId,
                    AnalyticsTimestamp = DateTime.UtcNow
                };

                // Get user basic info
                var user = await _context.Users
                    .Where(u => u.UserId == userId)
                    .Select(u => new { u.Fullname })
                    .FirstOrDefaultAsync();

                analytics.FullName = user?.Fullname ?? "";

                // Learning Overview with null safety
                var overviewData = new LearningOverviewDTO();
                
                var userCourseInfos = new List<UserCourseInfo>();
                try
                {
                    userCourseInfos = await _context.UserCourseInfos
                        .Where(uci => uci.UserId == userId)
                        .ToListAsync();
                }
                catch (Exception)
                {
                    // Continue with empty list if there's an issue
                    userCourseInfos = new List<UserCourseInfo>();
                }

                if (userCourseInfos.Any())
                {
                    overviewData.TotalCoursesEnrolled = userCourseInfos.Count;
                    overviewData.TotalCoursesCompleted = userCourseInfos.Count(uci => uci.LearningStatus == BusinessObjects.Enums.CourseLearningStatus.Completed);
                    overviewData.OverallProgressPercentage = userCourseInfos.Average(uci => (double)uci.ProgressPercent);
                }

                // Get learning content stats with null safety
                var userLearningProgresses = new List<UserLearningProgress>();
                try
                {
                    userLearningProgresses = await _context.UserLearningProgresses
                        .Where(ulp => ulp.UserId == userId)
                        .ToListAsync();
                }
                catch (Exception)
                {
                    // Continue with empty list if there's an issue
                    userLearningProgresses = new List<UserLearningProgress>();
                }

                if (userLearningProgresses.Any())
                {
                    overviewData.TotalLearningContentCompleted = userLearningProgresses.Count(ulp => ulp.Status == BusinessObjects.Enums.UserLearningProgressStatus.Completed);
                    
                    var completedWithTime = userLearningProgresses.Where(ulp => ulp.CompletedIn.HasValue).ToList();
                    if (completedWithTime.Any())
                    {
                        overviewData.TotalStudyTime = TimeSpan.FromMinutes(completedWithTime.Sum(ulp => ulp.CompletedIn!.Value.TotalMinutes));
                    }
                    
                    var progressesWithScore = userLearningProgresses.Where(ulp => ulp.Score > 0).ToList();
                    if (progressesWithScore.Any())
                    {
                        overviewData.AverageScorePercentage = progressesWithScore.Average(ulp => (double)ulp.Score);
                    }
                }

                // Get streak info with null safety
                var streakInfos = new List<UserCourseStreak>();
                try
                {
                    streakInfos = await _context.userCourseStreaks
                        .Where(ucs => ucs.UserId == userId)
                        .ToListAsync();
                }
                catch (Exception)
                {
                    // Continue with empty list if there's an issue with streak data
                    streakInfos = new List<UserCourseStreak>();
                }

                if (streakInfos.Any())
                {
                    overviewData.CurrentLongestStreak = streakInfos.Max(ucs => ucs.CurrentStreak);
                    overviewData.TotalStudyDays = streakInfos.Count;
                }

                analytics.LearningOverview = overviewData;

                // Simplified Course Progress to avoid complex joins
                analytics.CourseProgress = new List<DetailedCourseProgressDTO>();
                
                foreach (var courseInfo in userCourseInfos)
                {
                    try
                    {
                        var course = await _context.Courses
                            .Where(c => c.CourseId == courseInfo.CourseId)
                            .Include(c => c.CourseRegion)
                            .Include(c => c.CourseHistoricalPeriod)
                            .FirstOrDefaultAsync();

                        if (course == null) continue;

                        var courseProgress = new DetailedCourseProgressDTO
                        {
                            CourseId = courseInfo.CourseId,
                            CourseName = course.Title ?? "",
                            CourseDescription = course.Description ?? "",
                            ProgressPercentage = (double)courseInfo.ProgressPercent,
                            EnrolledDate = courseInfo.EnrolledAt ?? DateTime.UtcNow,
                            ArtPeriod = course.CourseHistoricalPeriod?.HistoricalPeriodName ?? "",
                            GeographicalRegion = course.CourseRegion?.RegionName ?? ""
                        };

                        // Get streak info for this course
                        var courseStreak = streakInfos.FirstOrDefault(si => si.CourseId == courseInfo.CourseId);
                        if (courseStreak != null)
                        {
                            courseProgress.StudyStreak = courseStreak.CurrentStreak;
                            courseProgress.LastStudyDate = courseStreak.LastAccessDate.ToDateTime(TimeOnly.MinValue);
                        }

                        // Get current module
                        var currentModule = await _context.UserModuleInfos
                            .Where(umi => umi.UserId == userId)
                            .Join(_context.Modules, umi => umi.ModuleId, m => m.ModuleId, (umi, m) => new { umi, m })
                            .Where(x => x.m.CourseId == courseInfo.CourseId && !x.umi.IsCompleted)
                            .OrderBy(x => x.m.ModuleId)
                            .Select(x => x.m.ModuleTitle)
                            .FirstOrDefaultAsync();

                        courseProgress.CurrentModule = currentModule ?? "Getting Started";

                        // Get module completion stats
                        var moduleStats = await _context.UserModuleInfos
                            .Where(umi => umi.UserId == userId)
                            .Join(_context.Modules, umi => umi.ModuleId, m => m.ModuleId, (umi, m) => new { umi, m })
                            .Where(x => x.m.CourseId == courseInfo.CourseId)
                            .GroupBy(x => 1)
                            .Select(g => new
                            {
                                ModulesCompleted = g.Count(x => x.umi.IsCompleted),
                                TotalModules = g.Count()
                            })
                            .FirstOrDefaultAsync();

                        if (moduleStats != null)
                        {
                            courseProgress.ModulesCompleted = moduleStats.ModulesCompleted;
                            courseProgress.TotalModules = moduleStats.TotalModules;
                        }

                        // Get recent topics for this course
                        var courseProgressData = userLearningProgresses
                            .Where(ulp => ulp.LearningContent != null && ulp.LearningContent.CourseId == courseInfo.CourseId)
                            .ToList();

                        var recentTopics = await _context.UserLearningProgresses
                            .Where(ulp => ulp.UserId == userId && 
                                         ulp.LearningContent!.CourseId == courseInfo.CourseId &&
                                         ulp.Status == BusinessObjects.Enums.UserLearningProgressStatus.Completed)
                            .Include(ulp => ulp.LearningContent)
                            .OrderByDescending(ulp => ulp.UpdatedAt)
                            .Take(5)
                            .Select(ulp => ulp.LearningContent!.Title ?? "")
                            .ToListAsync();

                        courseProgress.RecentlyCompletedTopics = recentTopics;

                        // Initialize required lists with default values
                        courseProgress.UpcomingTopics = new List<string>();
                        courseProgress.DifficultTopics = new List<string>();

                        // Get upcoming topics (next few learning contents for this course)
                        try
                        {
                            var upcomingTopics = await _context.LearningContents
                                .Where(lc => lc.CourseId == courseInfo.CourseId && 
                                           !_context.UserLearningProgresses.Any(ulp => 
                                               ulp.UserId == userId && 
                                               ulp.LearningContentId == lc.LearningContentId &&
                                               ulp.Status == BusinessObjects.Enums.UserLearningProgressStatus.Completed))
                                .OrderBy(lc => lc.DisplayOrder)
                                .Take(3)
                                .Select(lc => lc.Title ?? "")
                                .ToListAsync();
                            
                            courseProgress.UpcomingTopics = upcomingTopics;
                        }
                        catch
                        {
                            courseProgress.UpcomingTopics = new List<string>();
                        }

                        // Get difficult topics (topics with low scores or many attempts)
                        try
                        {
                            var difficultTopics = courseProgressData
                                .Where(ulp => ulp.LearningContent != null && 
                                            (ulp.Score < 70 || ulp.Attempts > 2))
                                .Select(ulp => ulp.LearningContent!.Title ?? "")
                                .Distinct()
                                .Take(3)
                                .ToList();
                            
                            courseProgress.DifficultTopics = difficultTopics;
                        }
                        catch
                        {
                            courseProgress.DifficultTopics = new List<string>();
                        }

                        // Get course performance
                        if (courseProgressData.Any())
                        {
                            var progressesWithScore = courseProgressData.Where(ulp => ulp.Score > 0).ToList();
                            if (progressesWithScore.Any())
                            {
                                courseProgress.AverageScore = progressesWithScore.Average(ulp => (double)ulp.Score);
                            }
                            
                            courseProgress.TotalAttempts = courseProgressData.Sum(ulp => ulp.Attempts);
                            
                            var progressesWithTime = courseProgressData.Where(ulp => ulp.CompletedIn.HasValue).ToList();
                            if (progressesWithTime.Any())
                            {
                                courseProgress.TotalTimeSpent = TimeSpan.FromMinutes(progressesWithTime.Sum(ulp => ulp.CompletedIn!.Value.TotalMinutes));
                            }
                        }

                        analytics.CourseProgress.Add(courseProgress);
                    }
                    catch (Exception)
                    {
                        // Log the error but continue with other courses
                        continue;
                    }
                }

                // Learning Patterns Analysis with error handling
                var learningPatterns = new LearningPatternsDTO();

                try
                {
                    // Analyze study time patterns
                    var studyTimes = userLearningProgresses
                        .Where(ulp => ulp.UpdatedAt.HasValue)
                        .Select(ulp => ulp.UpdatedAt!.Value.Hour)
                        .ToList();

                    if (studyTimes.Any())
                    {
                        var mostCommonHour = studyTimes.GroupBy(h => h).OrderByDescending(g => g.Count()).First().Key;
                        learningPatterns.PreferredStudyTime = mostCommonHour switch
                        {
                            >= 6 and < 12 => "Morning (6 AM - 12 PM)",
                            >= 12 and < 18 => "Afternoon (12 PM - 6 PM)",
                            _ => "Evening (6 PM - 12 AM)"
                        };
                    }

                    // Analyze session duration patterns
                    var sessionDurations = userLearningProgresses
                        .Where(ulp => ulp.CompletedIn.HasValue)
                        .Select(ulp => ulp.CompletedIn!.Value.TotalMinutes)
                        .ToList();

                    if (sessionDurations.Any())
                    {
                        learningPatterns.AverageSessionDuration = sessionDurations.Average();
                        var avgDuration = learningPatterns.AverageSessionDuration;
                        
                        learningPatterns.LearningStyle = avgDuration switch
                        {
                            < 10 => "Quick Learner - Prefers short, focused sessions",
                            > 30 => "Deep Learner - Enjoys comprehensive study sessions",
                            _ => "Balanced Learner - Adapts session length to content"
                        };
                    }

                    // Analyze study frequency
                    var weeklyStudyDays = streakInfos.Count(ucs => ucs.LastAccessDate >= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)));
                    learningPatterns.WeeklyStudyFrequency = weeklyStudyDays;

                    // Identify strong and weak areas with safety checks
                    var topicPerformance = await _context.UserLearningProgresses
                        .Where(ulp => ulp.UserId == userId && ulp.Score > 0)
                        .Include(ulp => ulp.LearningContent)
                        .GroupBy(ulp => ulp.LearningContent!.Title)
                        .Select(g => new
                        {
                            Topic = g.Key,
                            AverageScore = g.Average(ulp => (double)ulp.Score),
                            AttemptCount = g.Sum(ulp => ulp.Attempts)
                        })
                        .ToListAsync();

                    learningPatterns.StrongAreas = topicPerformance
                        .Where(tp => tp.AverageScore >= 85 && tp.AttemptCount <= 2)
                        .Select(tp => tp.Topic ?? "")
                        .Take(5)
                        .ToList();

                    learningPatterns.WeakAreas = topicPerformance
                        .Where(tp => tp.AverageScore < 70 || tp.AttemptCount > 3)
                        .Select(tp => tp.Topic ?? "")
                        .Take(5)
                        .ToList();

                    // Determine motivation level based on consistency
                    var consistencyScore = weeklyStudyDays * 10 + (overviewData.CurrentLongestStreak * 5);
                    learningPatterns.MotivationLevel = consistencyScore switch
                    {
                        >= 50 => "High - Excellent consistency and engagement",
                        >= 25 => "Medium - Good progress with room for improvement",
                        _ => "Low - Consider setting smaller, achievable goals"
                    };
                }
                catch (Exception)
                {
                    // Set default values if analysis fails
                    learningPatterns.PreferredStudyTime = "Not enough data";
                    learningPatterns.LearningStyle = "Adaptive";
                    learningPatterns.MotivationLevel = "Good - Keep up the progress!";
                    learningPatterns.StrongAreas = new List<string>();
                    learningPatterns.WeakAreas = new List<string>();
                }

                analytics.LearningPatterns = learningPatterns;

                // Generate simplified recommendations
                var recommendations = new RecommendationsDTO();
                
                recommendations.NextTopicsToStudy = analytics.CourseProgress
                    .SelectMany(cp => cp.UpcomingTopics.Take(2))
                    .Take(5)
                    .ToList();

                recommendations.TopicsToReview = learningPatterns.WeakAreas;

                if (overviewData.AverageScorePercentage < 75)
                    recommendations.SkillsToFocus.Add("Core concept understanding");
                if (learningPatterns.AverageSessionDuration < 5)
                    recommendations.SkillsToFocus.Add("Sustained attention and focus");
                if (learningPatterns.WeeklyStudyFrequency < 3)
                    recommendations.SkillsToFocus.Add("Consistent study habits");

                recommendations.StudyScheduleSuggestion = "Maintain regular study sessions for optimal learning progress.";
                recommendations.OptimalStudyPlan = "Focus on consistent progress through your enrolled courses.";
                
                if (overviewData.CurrentLongestStreak > 7)
                    recommendations.MotivationalInsights.Add($"Impressive {overviewData.CurrentLongestStreak}-day streak! You're building excellent study habits.");
                if (overviewData.TotalLearningContentCompleted > 20)
                    recommendations.MotivationalInsights.Add($"You've completed {overviewData.TotalLearningContentCompleted} learning modules - fantastic progress!");
                if (overviewData.AverageScorePercentage > 80)
                    recommendations.MotivationalInsights.Add("Your high average score shows excellent comprehension!");

                analytics.Recommendations = recommendations;

                return analytics;
            }
            catch (Exception)
            {
                // Return a basic analytics object if the detailed analysis fails
                return new UserLearningAnalyticsDTO
                {
                    UserId = userId,
                    AnalyticsTimestamp = DateTime.UtcNow,
                    FullName = "User",
                    LearningOverview = new LearningOverviewDTO(),
                    CourseProgress = new List<DetailedCourseProgressDTO>(),
                    LearningPatterns = new LearningPatternsDTO
                    {
                        PreferredStudyTime = "Not enough data",
                        LearningStyle = "Adaptive",
                        MotivationLevel = "Keep learning!",
                        StrongAreas = new List<string>(),
                        WeakAreas = new List<string>()
                    },
                    Recommendations = new RecommendationsDTO
                    {
                        NextTopicsToStudy = new List<string>(),
                        TopicsToReview = new List<string>(),
                        SkillsToFocus = new List<string> { "Continue your learning journey" },
                        StudyScheduleSuggestion = "Set aside regular time for studying",
                        MotivationalInsights = new List<string> { "Every expert was once a beginner!" },
                        OptimalStudyPlan = "Start with courses that interest you most"
                    }
                };
            }
        }
    }
}
