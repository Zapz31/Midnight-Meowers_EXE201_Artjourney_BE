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

            // Learning Overview
            var overviewData = await _context.UserCourseInfos
                .Where(uci => uci.UserId == userId)
                .GroupBy(uci => 1)
                .Select(g => new LearningOverviewDTO
                {
                    TotalCoursesEnrolled = g.Count(),
                    TotalCoursesCompleted = g.Count(uci => uci.LearningStatus == BusinessObjects.Enums.CourseLearningStatus.Completed),
                    OverallProgressPercentage = g.Average(uci => (double)uci.ProgressPercent)
                })
                .FirstOrDefaultAsync() ?? new LearningOverviewDTO();

            // Get learning content stats
            var contentStats = await _context.UserLearningProgresses
                .Where(ulp => ulp.UserId == userId)
                .GroupBy(ulp => 1)
                .Select(g => new
                {
                    TotalCompleted = g.Count(ulp => ulp.Status == BusinessObjects.Enums.UserLearningProgressStatus.Completed),
                    TotalStudyTime = g.Where(ulp => ulp.CompletedIn.HasValue).Sum(ulp => ulp.CompletedIn!.Value.TotalMinutes),
                    AverageScore = g.Where(ulp => ulp.Score > 0).Average(ulp => (double)ulp.Score)
                })
                .FirstOrDefaultAsync();

            overviewData.TotalLearningContentCompleted = contentStats?.TotalCompleted ?? 0;
            overviewData.TotalStudyTime = TimeSpan.FromMinutes(contentStats?.TotalStudyTime ?? 0);
            overviewData.AverageScorePercentage = contentStats?.AverageScore ?? 0;

            // Get streak info
            var streakInfo = await _context.userCourseStreaks
                .Where(ucs => ucs.UserId == userId)
                .MaxAsync(ucs => (int?)ucs.CurrentStreak) ?? 0;

            overviewData.CurrentLongestStreak = streakInfo;
            overviewData.TotalStudyDays = await _context.userCourseStreaks
                .Where(ucs => ucs.UserId == userId)
                .CountAsync();

            analytics.LearningOverview = overviewData;

            // Detailed Course Progress
            var courseProgressQuery = from uci in _context.UserCourseInfos
                                    where uci.UserId == userId
                                    select new
                                    {
                                        uci.CourseId,
                                        uci.Course!.Title,
                                        uci.Course.Description,
                                        ProgressPercentage = (double)uci.ProgressPercent,
                                        uci.EnrolledAt,
                                        RegionName = uci.Course.CourseRegion!.RegionName,
                                        PeriodName = uci.Course.CourseHistoricalPeriod!.HistoricalPeriodName,
                                        // Get current module
                                        CurrentModule = (from umi in _context.UserModuleInfos
                                                       join m in _context.Modules on umi.ModuleId equals m.ModuleId
                                                       where umi.UserId == userId && 
                                                             m.CourseId == uci.CourseId && 
                                                             !umi.IsCompleted
                                                       select m.ModuleTitle).FirstOrDefault(),
                                        // Get streak for this course
                                        CourseStreak = (from ucs in _context.userCourseStreaks
                                                      where ucs.UserId == userId && ucs.CourseId == uci.CourseId
                                                      select new { ucs.CurrentStreak, ucs.LastAccessDate }).FirstOrDefault()
                                    };

            var courseProgressData = await courseProgressQuery.ToListAsync();

            analytics.CourseProgress = (await Task.WhenAll(courseProgressData.Select(async cp =>
            {
                // Get recent completed topics for this course
                var recentTopics = await _context.UserLearningProgresses
                    .Where(ulp => ulp.UserId == userId && 
                                 ulp.LearningContent!.CourseId == cp.CourseId &&
                                 ulp.Status == BusinessObjects.Enums.UserLearningProgressStatus.Completed)
                    .OrderByDescending(ulp => ulp.UpdatedAt)
                    .Take(5)
                    .Select(ulp => ulp.LearningContent!.Title ?? "")
                    .ToListAsync();

                // Get upcoming topics (not started or in progress)
                var upcomingTopics = await _context.UserLearningProgresses
                    .Where(ulp => ulp.UserId == userId && 
                                 ulp.LearningContent!.CourseId == cp.CourseId &&
                                 (ulp.Status == BusinessObjects.Enums.UserLearningProgressStatus.NotStarted ||
                                  ulp.Status == BusinessObjects.Enums.UserLearningProgressStatus.InProgress))
                    .Take(5)
                    .Select(ulp => ulp.LearningContent!.Title ?? "")
                    .ToListAsync();

                // Get difficult topics (multiple attempts or low scores)
                var difficultTopics = await _context.UserLearningProgresses
                    .Where(ulp => ulp.UserId == userId && 
                                 ulp.LearningContent!.CourseId == cp.CourseId &&
                                 (ulp.Attempts > 2 || ulp.Score < 70))
                    .Select(ulp => ulp.LearningContent!.Title ?? "")
                    .ToListAsync();

                // Get course performance metrics
                var coursePerformance = await _context.UserLearningProgresses
                    .Where(ulp => ulp.UserId == userId && ulp.LearningContent!.CourseId == cp.CourseId)
                    .GroupBy(ulp => 1)
                    .Select(g => new
                    {
                        AverageScore = g.Where(ulp => ulp.Score > 0).Average(ulp => (double)ulp.Score),
                        TotalAttempts = g.Sum(ulp => ulp.Attempts),
                        TotalTimeSpent = g.Where(ulp => ulp.CompletedIn.HasValue).Sum(ulp => ulp.CompletedIn!.Value.TotalMinutes)
                    })
                    .FirstOrDefaultAsync();

                // Get module completion info
                var moduleInfo = await _context.UserModuleInfos
                    .Where(umi => umi.UserId == userId)
                    .Join(_context.Modules, umi => umi.ModuleId, m => m.ModuleId, (umi, m) => new { umi, m })
                    .Where(x => x.m.CourseId == cp.CourseId)
                    .GroupBy(x => 1)
                    .Select(g => new
                    {
                        ModulesCompleted = g.Count(x => x.umi.IsCompleted),
                        TotalModules = g.Count()
                    })
                    .FirstOrDefaultAsync();

                return new DetailedCourseProgressDTO
                {
                    CourseId = cp.CourseId,
                    CourseName = cp.Title ?? "",
                    CourseDescription = cp.Description ?? "",
                    ProgressPercentage = cp.ProgressPercentage,
                    EnrolledDate = cp.EnrolledAt ?? DateTime.UtcNow,
                    LastStudyDate = cp.CourseStreak?.LastAccessDate.ToDateTime(TimeOnly.MinValue),
                    StudyStreak = cp.CourseStreak?.CurrentStreak ?? 0,
                    TotalTimeSpent = TimeSpan.FromMinutes(coursePerformance?.TotalTimeSpent ?? 0),
                    CurrentModule = cp.CurrentModule ?? "Getting Started",
                    ModulesCompleted = moduleInfo?.ModulesCompleted ?? 0,
                    TotalModules = moduleInfo?.TotalModules ?? 0,
                    AverageScore = coursePerformance?.AverageScore ?? 0,
                    TotalAttempts = coursePerformance?.TotalAttempts ?? 0,
                    RecentlyCompletedTopics = recentTopics,
                    UpcomingTopics = upcomingTopics,
                    DifficultTopics = difficultTopics,
                    ArtPeriod = cp.PeriodName ?? "",
                    GeographicalRegion = cp.RegionName ?? ""
                };
            }))).ToList();

            // Learning Patterns Analysis
            var learningPatterns = new LearningPatternsDTO();

            // Analyze study time patterns
            var studyTimes = await _context.UserLearningProgresses
                .Where(ulp => ulp.UserId == userId && ulp.UpdatedAt.HasValue)
                .Select(ulp => ulp.UpdatedAt!.Value.Hour)
                .ToListAsync();

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
            var sessionDurations = await _context.UserLearningProgresses
                .Where(ulp => ulp.UserId == userId && ulp.CompletedIn.HasValue)
                .Select(ulp => ulp.CompletedIn!.Value.TotalMinutes)
                .ToListAsync();

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
            var weeklyStudyDays = await _context.userCourseStreaks
                .Where(ucs => ucs.UserId == userId && ucs.LastAccessDate >= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)))
                .CountAsync();

            learningPatterns.WeeklyStudyFrequency = weeklyStudyDays;

            // Identify strong and weak areas
            var topicPerformance = await _context.UserLearningProgresses
                .Where(ulp => ulp.UserId == userId && ulp.Score > 0)
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

            analytics.LearningPatterns = learningPatterns;

            // Generate Recommendations
            var recommendations = new RecommendationsDTO();

            // Suggest next topics based on current progress
            recommendations.NextTopicsToStudy = analytics.CourseProgress
                .SelectMany(cp => cp.UpcomingTopics.Take(2))
                .Take(5)
                .ToList();

            // Topics that need review (difficult areas)
            recommendations.TopicsToReview = learningPatterns.WeakAreas;

            // Skills to focus on based on overall performance
            if (overviewData.AverageScorePercentage < 75)
                recommendations.SkillsToFocus.Add("Core concept understanding");
            if (learningPatterns.AverageSessionDuration < 5)
                recommendations.SkillsToFocus.Add("Sustained attention and focus");
            if (weeklyStudyDays < 3)
                recommendations.SkillsToFocus.Add("Consistent study habits");

            // Study schedule suggestion
            recommendations.StudyScheduleSuggestion = learningPatterns.PreferredStudyTime switch
            {
                var t when t.Contains("Morning") => "Continue your morning study routine - you're most productive then! Aim for 20-30 minute sessions.",
                var t when t.Contains("Afternoon") => "Your afternoon study sessions work well. Try to maintain 2-3 sessions per week.",
                _ => "Evening study suits you. Consider shorter sessions (15-20 min) to maintain focus after a long day."
            };

            // Motivational insights
            if (overviewData.CurrentLongestStreak > 7)
                recommendations.MotivationalInsights.Add($"Impressive {overviewData.CurrentLongestStreak}-day streak! You're building excellent study habits.");
            if (overviewData.TotalLearningContentCompleted > 20)
                recommendations.MotivationalInsights.Add($"You've completed {overviewData.TotalLearningContentCompleted} learning modules - fantastic progress!");
            if (overviewData.AverageScorePercentage > 80)
                recommendations.MotivationalInsights.Add("Your high average score shows excellent comprehension!");

            // Optimal study plan
            var totalProgress = analytics.CourseProgress.Average(cp => cp.ProgressPercentage);
            recommendations.OptimalStudyPlan = totalProgress switch
            {
                < 25 => "Focus on building foundational knowledge. Aim for 15-20 minutes daily on core concepts.",
                < 50 => "You're making good progress! Continue with regular study sessions, adding review time for difficult topics.",
                < 75 => "You're in the advanced stage! Focus on connecting concepts and preparing for assessments.",
                _ => "Excellent progress! Consider exploring advanced topics and real-world applications."
            };

            analytics.Recommendations = recommendations;

            return analytics;
        }
    }
}
