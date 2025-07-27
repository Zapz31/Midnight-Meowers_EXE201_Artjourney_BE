using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.Courses;
using Helpers.HelperClasses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAOs;

namespace Services.Implements
{
    public class CourseRecommendationService : ICourseRecommendationService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUserCourseInfoRepository _userCourseInfoRepository;
        private readonly IUserOptionChoiceRepository _userOptionChoiceRepository;
        private readonly ILogger<CourseRecommendationService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;

        public CourseRecommendationService(
            ICourseRepository courseRepository,
            IUserCourseInfoRepository userCourseInfoRepository,
            IUserOptionChoiceRepository userOptionChoiceRepository,
            ILogger<CourseRecommendationService> logger,
            ICurrentUserService currentUserService,
            ApplicationDbContext context)
        {
            _courseRepository = courseRepository;
            _userCourseInfoRepository = userCourseInfoRepository;
            _userOptionChoiceRepository = userOptionChoiceRepository;
            _logger = logger;
            _currentUserService = currentUserService;
            _context = context;
        }

        public async Task<ApiResponse<List<CourseRecommendationDTO>>> GetSurveyBasedRecommendationsAsync(long userId)
        {
            try
            {
                // Get user's survey responses using raw SQL for better control
                var userPreferences = await BuildUserPreferenceProfileAsync(userId);
                
                if (userPreferences.PreferredRegions.Count == 0 && 
                    userPreferences.PreferredHistoricalPeriods.Count == 0 && 
                    string.IsNullOrEmpty(userPreferences.KnowledgeLevel))
                {
                    // No survey data, return empty result
                    return new ApiResponse<List<CourseRecommendationDTO>>
                    {
                        Status = ResponseStatus.Success,
                        Code = 200,
                        Data = new List<CourseRecommendationDTO>(),
                        Message = "No survey data found for recommendations"
                    };
                }

                // Get all available courses
                var allCoursesResponse = await _courseRepository.GetAllPublishedCoursesGroupedByRegionAsync();
                var allCourses = allCoursesResponse.SelectMany(r => r.Courses).ToList();

                // Get user's enrolled courses to exclude them
                var allCourseIds = allCourses.Select(c => c.CourseId).ToList();
                var enrolledCourses = await _userCourseInfoRepository.GetUserCourseInfosByUserIdAndCourseIds(userId, allCourseIds);
                var enrolledCourseIds = enrolledCourses.Select(uc => uc.CourseId).ToHashSet();

                var recommendations = new List<CourseRecommendationDTO>();

                foreach (var course in allCourses)
                {
                    // Skip if user already enrolled
                    if (enrolledCourseIds.Contains(course.CourseId))
                        continue;

                    var score = CalculateSurveyBasedScore(course, userPreferences);
                    if (score > 0)
                    {
                        var recommendation = MapToRecommendationDTO(course, score, 
                            GenerateSurveyBasedReason(course, userPreferences), "Survey", userPreferences);
                        recommendations.Add(recommendation);
                    }
                }

                // Sort by recommendation score and take top recommendations
                var topRecommendations = recommendations
                    .OrderByDescending(r => r.RecommendationScore)
                    .Take(10)
                    .ToList();

                return new ApiResponse<List<CourseRecommendationDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = topRecommendations,
                    Message = "Survey-based recommendations retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting survey-based recommendations: {Error}", ex.Message);
                return new ApiResponse<List<CourseRecommendationDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "Error retrieving recommendations"
                };
            }
        }

        public async Task<ApiResponse<List<CourseRecommendationDTO>>> GetEnrollmentBasedRecommendationsAsync(long userId)
        {
            try
            {
                // Get user's enrollment history using the correct method
                var enrolledCoursesHistory = await _courseRepository.GetCoursesHasEnrolledByUserIdAsync(userId);
                
                if (!enrolledCoursesHistory.Any())
                {
                    // No enrollment history, return empty result
                    return new ApiResponse<List<CourseRecommendationDTO>>
                    {
                        Status = ResponseStatus.Success,
                        Code = 200,
                        Data = new List<CourseRecommendationDTO>(),
                        Message = "No enrollment history found for recommendations"
                    };
                }

                // Build preference profile from enrollment history
                var userPreferences = new UserPreferenceProfileDTO { UserId = userId };
                foreach (var course in enrolledCoursesHistory)
                {
                    userPreferences.EnrolledCourseRegions.Add(course.RegionName ?? "");
                    userPreferences.EnrolledCoursePeriods.Add(course.HistorialPeriodName ?? "");
                    
                    if (course.CompletedAt.HasValue)
                    {
                        userPreferences.CompletedCourseRegions.Add(course.RegionName ?? "");
                        userPreferences.CompletedCoursePeriods.Add(course.HistorialPeriodName ?? "");
                    }
                }
                
                // Get all available courses
                var allCoursesResponse = await _courseRepository.GetAllPublishedCoursesGroupedByRegionAsync();
                var allCourses = allCoursesResponse.SelectMany(r => r.Courses).ToList();

                // Get enrolled course IDs to exclude them
                var enrolledCourseIds = enrolledCoursesHistory.Select(c => c.CourseId).ToHashSet();

                var recommendations = new List<CourseRecommendationDTO>();

                foreach (var course in allCourses)
                {
                    // Skip if user already enrolled
                    if (enrolledCourseIds.Contains(course.CourseId))
                        continue;

                    var score = CalculateEnrollmentBasedScore(course, userPreferences);
                    if (score > 0)
                    {
                        var recommendation = MapToRecommendationDTO(course, score, 
                            GenerateEnrollmentBasedReason(course, userPreferences), "Enrollment", userPreferences);
                        recommendations.Add(recommendation);
                    }
                }

                // Sort by recommendation score and take top recommendations
                var topRecommendations = recommendations
                    .OrderByDescending(r => r.RecommendationScore)
                    .Take(10)
                    .ToList();

                return new ApiResponse<List<CourseRecommendationDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = topRecommendations,
                    Message = "Enrollment-based recommendations retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting enrollment-based recommendations: {Error}", ex.Message);
                return new ApiResponse<List<CourseRecommendationDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "Error retrieving recommendations"
                };
            }
        }

        public async Task<ApiResponse<List<CourseRecommendationDTO>>> GetPersonalizedRecommendationsAsync(long userId)
        {
            try
            {
                // Get both survey and enrollment based recommendations
                var surveyRecommendations = await GetSurveyBasedRecommendationsAsync(userId);
                var enrollmentRecommendations = await GetEnrollmentBasedRecommendationsAsync(userId);

                var combinedRecommendations = new Dictionary<long, CourseRecommendationDTO>();

                // Add survey-based recommendations
                if (surveyRecommendations.Data != null)
                {
                    foreach (var rec in surveyRecommendations.Data)
                    {
                        combinedRecommendations[rec.CourseId] = rec;
                    }
                }

                // Merge enrollment-based recommendations
                if (enrollmentRecommendations.Data != null)
                {
                    foreach (var rec in enrollmentRecommendations.Data)
                    {
                        if (combinedRecommendations.ContainsKey(rec.CourseId))
                        {
                            // Combine scores and reasons
                            var existing = combinedRecommendations[rec.CourseId];
                            existing.RecommendationScore = (existing.RecommendationScore + rec.RecommendationScore) / 2;
                            existing.RecommendationReason += $" | {rec.RecommendationReason}";
                            existing.RecommendationSource = "Survey + Enrollment";
                            existing.MatchedPreferences.AddRange(rec.MatchedPreferences);
                            existing.MatchedPreferences = existing.MatchedPreferences.Distinct().ToList();
                        }
                        else
                        {
                            combinedRecommendations[rec.CourseId] = rec;
                        }
                    }
                }

                // If user has no data, provide trending courses
                if (!combinedRecommendations.Any())
                {
                    var trendingResponse = await GetTrendingCoursesAsync(userId);
                    return trendingResponse;
                }

                var finalRecommendations = combinedRecommendations.Values
                    .OrderByDescending(r => r.RecommendationScore)
                    .Take(12)
                    .ToList();

                return new ApiResponse<List<CourseRecommendationDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = finalRecommendations,
                    Message = "Personalized recommendations retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting personalized recommendations: {Error}", ex.Message);
                return new ApiResponse<List<CourseRecommendationDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "Error retrieving recommendations"
                };
            }
        }

        public async Task<ApiResponse<List<CourseRecommendationDTO>>> GetTrendingCoursesAsync(long? userId = null)
        {
            try
            {
                var allCoursesResponse = await _courseRepository.GetAllPublishedCoursesGroupedByRegionAsync();
                var allCourses = allCoursesResponse.SelectMany(r => r.Courses).ToList();

                // If userId is provided, filter out enrolled courses
                if (userId.HasValue)
                {
                    var allCourseIds = allCourses.Select(c => c.CourseId).ToList();
                    var enrolledCourses = await _userCourseInfoRepository.GetUserCourseInfosByUserIdAndCourseIds(userId.Value, allCourseIds);
                    var enrolledCourseIds = enrolledCourses.Select(uc => uc.CourseId).ToHashSet();
                    allCourses = allCourses.Where(c => !enrolledCourseIds.Contains(c.CourseId)).ToList();
                }

                // Use average rating and total feedbacks instead of enrollment count
                var trendingCourses = allCourses
                    .Where(c => c.AverageRating > 0 || c.TotalFeedbacks > 0) // Only include courses with ratings/feedback
                    .OrderByDescending(c => c.AverageRating) // Primary sort by average rating
                    .ThenByDescending(c => c.TotalFeedbacks) // Secondary sort by total feedback count
                    .Take(12)
                    .Select(course => new CourseRecommendationDTO
                    {
                        CourseId = course.CourseId,
                        CourseName = course.Title ?? string.Empty,
                        Description = course.Description ?? string.Empty,
                        ThumbnailUrl = course.ThumbnailImageUrl ?? string.Empty,
                        CoverImageUrl = course.CoverImageUrl ?? string.Empty,
                        RegionName = course.RegionName ?? string.Empty,
                        HistoricalPeriodName = course.HistoricalPeriodName ?? string.Empty,
                        IsPremium = course.IsPremium ?? false,
                        Level = course.Level.ToString(),
                        Price = course.Price,
                        AverageRating = course.AverageRating,
                        EnrollmentCount = course.TotalFeedbacks, // Use TotalFeedbacks instead of EnrollmentCount
                        RecommendationScore = CalculateTrendingScore(course),
                        RecommendationReason = $"Trending course with {course.AverageRating:F1}★ rating and {course.TotalFeedbacks} reviews",
                        RecommendationSource = "Trending",
                        MatchedPreferences = new List<string> { "Popular", "High-rated" }
                    })
                    .ToList();

                return new ApiResponse<List<CourseRecommendationDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = trendingCourses,
                    Message = "Trending courses retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting trending courses: {Error}", ex.Message);
                return new ApiResponse<List<CourseRecommendationDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "Error retrieving trending courses"
                };
            }
        }

        public async Task<ApiResponse<List<CourseRecommendationDTO>>> GetBeginnerRecommendationsAsync()
        {
            try
            {
                var allCoursesResponse = await _courseRepository.GetAllPublishedCoursesGroupedByRegionAsync();
                var allCourses = allCoursesResponse.SelectMany(r => r.Courses).ToList();

                var beginnerCourses = allCourses
                    .Where(c => c.Level == CourseLevel.Easy) // Use correct enum value
                    .OrderByDescending(c => c.AverageRating)
                    .ThenByDescending(c => c.EnrollmentCount)
                    .Take(10)
                    .Select(course => new CourseRecommendationDTO
                    {
                        CourseId = course.CourseId,
                        CourseName = course.Title ?? string.Empty,
                        Description = course.Description ?? string.Empty,
                        ThumbnailUrl = course.ThumbnailUrl ?? string.Empty,
                        CoverImageUrl = course.CoverImageUrl ?? string.Empty,
                        RegionName = course.RegionName ?? string.Empty,
                        HistoricalPeriodName = course.HistoricalPeriodName ?? string.Empty,
                        IsPremium = course.IsPremium ?? false,
                        Level = course.Level.ToString(),
                        Price = course.Price,
                        AverageRating = course.AverageRating,
                        EnrollmentCount = course.EnrollmentCount,
                        RecommendationScore = 80,
                        RecommendationReason = "Perfect for beginners - well-structured and highly rated",
                        RecommendationSource = "Beginner",
                        MatchedPreferences = new List<string> { "Beginner-friendly", "Highly-rated" }
                    })
                    .ToList();

                return new ApiResponse<List<CourseRecommendationDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = beginnerCourses,
                    Message = "Beginner courses retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting beginner courses: {Error}", ex.Message);
                return new ApiResponse<List<CourseRecommendationDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "Error retrieving beginner courses"
                };
            }
        }

        private async Task<UserPreferenceProfileDTO> BuildUserPreferenceProfileAsync(long userId)
        {
            var profile = new UserPreferenceProfileDTO { UserId = userId };

            try
            {
                // Get user's survey responses
                var userChoices = await _userOptionChoiceRepository.GetUserChoicesByUserIdAsync(userId);

                foreach (var choice in userChoices)
                {
                    var questionContent = choice.SurveyOption?.SurveyQuestion?.SurveyQuestionName ?? "";
                    var optionContent = choice.SurveyOption?.SurveyOptionContent ?? "";

                    // Map survey responses to preferences based on your survey structure
                    MapSurveyResponseToPreferences(profile, questionContent, optionContent);
                }

                // Get user's enrollment history
                var enrolledCourses = await _userCourseInfoRepository.GetUserCourseInfosByUserIdAndCourseIds(userId, new List<long>());

                if (enrolledCourses.Any())
                {
                    var enrolledCoursesDetails = await _courseRepository.GetCoursesHasEnrolledByUserIdAsync(userId);
                    foreach (var course in enrolledCoursesDetails)
                    {
                        profile.EnrolledCourseRegions.Add(course.RegionName ?? "");
                        profile.EnrolledCoursePeriods.Add(course.HistorialPeriodName ?? "");

                        // Check if completed
                        var courseInfo = enrolledCourses.FirstOrDefault(uc => uc.CourseId == course.CourseId);
                        if (courseInfo?.LearningStatus == CourseLearningStatus.Completed)
                        {
                            profile.CompletedCourseRegions.Add(course.RegionName ?? "");
                            profile.CompletedCoursePeriods.Add(course.HistorialPeriodName ?? "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error building user preference profile: {Error}", ex.Message);
            }

            return profile;
        }

        private void MapSurveyResponseToPreferences(UserPreferenceProfileDTO profile, string questionContent, string optionContent)
        {
            // Map based on your survey structure
            if (questionContent.Contains("continent's art history interests"))
            {
                profile.PreferredRegions.Add(optionContent);
            }
            else if (questionContent.Contains("current level of knowledge"))
            {
                profile.KnowledgeLevel = optionContent;
            }
            else if (questionContent.Contains("time periods in art history fascinate"))
            {
                profile.PreferredHistoricalPeriods.Add(optionContent);
            }
            else if (questionContent.Contains("type of art content interests"))
            {
                profile.PreferredArtTypes.Add(optionContent);
            }
            else if (questionContent.Contains("prefer to learn about art history"))
            {
                profile.PreferredLearningStyles.Add(optionContent);
            }
            else if (questionContent.Contains("motivates you to learn"))
            {
                profile.LearningMotivation = optionContent;
            }
            else if (questionContent.Contains("time would you like to dedicate"))
            {
                profile.PreferredTimeCommitment = optionContent;
            }
            else if (questionContent.Contains("art movements or styles"))
            {
                profile.PreferredArtMovements.Add(optionContent);
            }
        }

        private int CalculateSurveyBasedScore(CourseDTO course, UserPreferenceProfileDTO preferences)
        {
            int score = 0;

            // Region matching (30 points max)
            if (preferences.PreferredRegions.Any(region => 
                region.Contains("Europe") && course.RegionName?.Contains("Europe") == true ||
                region.Contains("Asia") && course.RegionName?.Contains("Asia") == true ||
                region.Contains("Africa") && course.RegionName?.Contains("Africa") == true ||
                region.Contains("North America") && course.RegionName?.Contains("North America") == true ||
                region.Contains("South America") && course.RegionName?.Contains("South America") == true ||
                region.Contains("Oceania") && course.RegionName?.Contains("Oceania") == true))
            {
                score += 30;
            }

            // Historical period matching (25 points max)
            if (preferences.PreferredHistoricalPeriods.Any(period => 
                course.HistoricalPeriodName?.ToLower().Contains(period.ToLower()) == true))
            {
                score += 25;
            }

            // Knowledge level matching (20 points max) - Fix the logic here
            if (!string.IsNullOrEmpty(preferences.KnowledgeLevel))
            {
                // Map survey responses correctly to CourseLevel enum
                if ((preferences.KnowledgeLevel.ToLower().Contains("beginner") || 
                     preferences.KnowledgeLevel.ToLower().Contains("novice") ||
                     preferences.KnowledgeLevel.ToLower().Contains("basic")) && 
                    course.Level == CourseLevel.Easy)
                    score += 20;
                else if ((preferences.KnowledgeLevel.ToLower().Contains("intermediate") || 
                          preferences.KnowledgeLevel.ToLower().Contains("moderate") ||
                          preferences.KnowledgeLevel.ToLower().Contains("some knowledge")) && 
                         course.Level == CourseLevel.Medium)
                    score += 20;
                else if ((preferences.KnowledgeLevel.ToLower().Contains("advanced") || 
                          preferences.KnowledgeLevel.ToLower().Contains("expert") ||
                          preferences.KnowledgeLevel.ToLower().Contains("extensive")) && 
                         course.Level == CourseLevel.Hard)
                    score += 20;
            }

            // Art movement matching (15 points max)
            foreach (var movement in preferences.PreferredArtMovements)
            {
                if (course.Title?.ToLower().Contains(movement.ToLower()) == true ||
                    course.Description?.ToLower().Contains(movement.ToLower()) == true)
                {
                    score += 15;
                    break;
                }
            }

            // Quality bonus based on actual ratings (10 points max)
            if (course.AverageRating >= 4.5m) score += 10;
            else if (course.AverageRating >= 4.0m) score += 8;
            else if (course.AverageRating >= 3.5m) score += 5;
            else if (course.AverageRating >= 3.0m) score += 3;

            return Math.Min(score, 100);
        }

        private int CalculateEnrollmentBasedScore(CourseDTO course, UserPreferenceProfileDTO preferences)
        {
            int score = 0;

            // Same region as enrolled courses (40 points max)
            if (!string.IsNullOrEmpty(course.RegionName) && preferences.EnrolledCourseRegions.Contains(course.RegionName))
            {
                score += 40;
            }

            // Same historical period as enrolled courses (30 points max)
            if (!string.IsNullOrEmpty(course.HistoricalPeriodName) && preferences.EnrolledCoursePeriods.Contains(course.HistoricalPeriodName))
            {
                score += 30;
            }

            // Same region as completed courses (bonus) (20 points max)
            if (!string.IsNullOrEmpty(course.RegionName) && preferences.CompletedCourseRegions.Contains(course.RegionName))
            {
                score += 20;
            }

            // Quality and popularity (10 points max) - Use actual ratings
            if (course.AverageRating >= 4.5m) score += 10;
            else if (course.AverageRating >= 4.0m) score += 8;
            else if (course.AverageRating >= 3.5m) score += 5;
            else if (course.AverageRating >= 3.0m) score += 3;

            return Math.Min(score, 100);
        }

        private int CalculateTrendingScore(CourseDTO course)
        {
            int score = 0;

            // Base trending score using actual ratings (50 points max)
            if (course.AverageRating >= 4.5m) score += 50;
            else if (course.AverageRating >= 4.0m) score += 40;
            else if (course.AverageRating >= 3.5m) score += 30;
            else if (course.AverageRating >= 3.0m) score += 20;
            else score += 10; // Minimum score for published courses

            // Level-based scoring - beginners prefer easier courses (30 points max)
            switch (course.Level)
            {
                case CourseLevel.Easy:
                    score += 25; // Beginner-friendly courses tend to be more popular
                    break;
                case CourseLevel.Medium:
                    score += 20;
                    break;
                case CourseLevel.Hard:
                    score += 15;
                    break;
            }

            // Total feedback bonus (20 points max) - use TotalFeedbacks instead of EnrollmentCount
            if (course.TotalFeedbacks > 0)
            {
                if (course.TotalFeedbacks >= 100) score += 20;
                else if (course.TotalFeedbacks >= 50) score += 15;
                else if (course.TotalFeedbacks >= 20) score += 10;
                else if (course.TotalFeedbacks >= 10) score += 5;
            }
            else
            {
                // If total feedback is 0, give a base score for new courses
                score += 5;
            }

            return Math.Min(score, 100);
        }

        private CourseRecommendationDTO MapToRecommendationDTO(CourseDTO course, int score, string reason, string source, UserPreferenceProfileDTO preferences)
        {
            var matchedPrefs = new List<string>();

            // Add matched preferences
            if (preferences.PreferredRegions.Any(r => course.RegionName?.Contains(ExtractRegionKeyword(r)) == true))
                matchedPrefs.Add("Region");
            if (preferences.PreferredHistoricalPeriods.Any(p => course.HistoricalPeriodName?.ToLower().Contains(p.ToLower()) == true))
                matchedPrefs.Add("Historical Period");
            if (!string.IsNullOrEmpty(preferences.KnowledgeLevel) && IsLevelMatch(preferences.KnowledgeLevel, course.Level))
                matchedPrefs.Add("Knowledge Level");

            return new CourseRecommendationDTO
            {
                CourseId = course.CourseId,
                CourseName = course.Title ?? string.Empty,
                Description = course.Description ?? string.Empty,
                ThumbnailUrl = course.ThumbnailUrl ?? string.Empty,
                CoverImageUrl = course.CoverImageUrl ?? string.Empty,
                RegionName = course.RegionName ?? string.Empty,
                HistoricalPeriodName = course.HistoricalPeriodName ?? string.Empty,
                IsPremium = course.IsPremium ?? false,
                Level = course.Level.ToString(),
                Price = course.Price,
                AverageRating = course.AverageRating,
                EnrollmentCount = course.EnrollmentCount,
                RecommendationScore = score,
                RecommendationReason = reason,
                RecommendationSource = source,
                MatchedPreferences = matchedPrefs
            };
        }

        private bool IsLevelMatch(string knowledgeLevel, CourseLevel courseLevel)
        {
            var level = knowledgeLevel.ToLower();
            return (level.Contains("beginner") || level.Contains("novice") || level.Contains("basic")) && courseLevel == CourseLevel.Easy ||
                   (level.Contains("intermediate") || level.Contains("moderate") || level.Contains("some knowledge")) && courseLevel == CourseLevel.Medium ||
                   (level.Contains("advanced") || level.Contains("expert") || level.Contains("extensive")) && courseLevel == CourseLevel.Hard;
        }

        private string GenerateSurveyBasedReason(CourseDTO course, UserPreferenceProfileDTO preferences)
        {
            var reasons = new List<string>();

            if (preferences.PreferredRegions.Any(r => course.RegionName?.Contains(ExtractRegionKeyword(r)) == true))
                reasons.Add($"matches your interest in {course.RegionName} art");

            if (preferences.PreferredHistoricalPeriods.Any(p => course.HistoricalPeriodName?.ToLower().Contains(p.ToLower()) == true))
                reasons.Add($"covers {course.HistoricalPeriodName} period you're interested in");

            if (!string.IsNullOrEmpty(preferences.KnowledgeLevel) && IsLevelMatch(preferences.KnowledgeLevel, course.Level))
                reasons.Add($"matches your {preferences.KnowledgeLevel} level");

            if (course.AverageRating >= 4.0m)
                reasons.Add($"has excellent ratings ({course.AverageRating:F1}★)");

            return reasons.Any() ? $"This course {string.Join(", ", reasons)}" : "Recommended based on your preferences";
        }

        private string GenerateEnrollmentBasedReason(CourseDTO course, UserPreferenceProfileDTO preferences)
        {
            var reasons = new List<string>();

            if (!string.IsNullOrEmpty(course.RegionName) && preferences.EnrolledCourseRegions.Contains(course.RegionName))
                reasons.Add($"similar to your {course.RegionName} courses");

            if (!string.IsNullOrEmpty(course.HistoricalPeriodName) && preferences.EnrolledCoursePeriods.Contains(course.HistoricalPeriodName))
                reasons.Add($"continues your {course.HistoricalPeriodName} studies");

            if (!string.IsNullOrEmpty(course.RegionName) && preferences.CompletedCourseRegions.Contains(course.RegionName))
                reasons.Add($"builds on your completed {course.RegionName} knowledge");

            return reasons.Any() ? $"Recommended because it {string.Join(" and ", reasons)}" : "Based on your learning history";
        }

        private string ExtractRegionKeyword(string regionResponse)
        {
            if (regionResponse.Contains("Europe")) return "Europe";
            if (regionResponse.Contains("Asia")) return "Asia";
            if (regionResponse.Contains("Africa")) return "Africa";
            if (regionResponse.Contains("North America")) return "North America";
            if (regionResponse.Contains("South America")) return "South America";
            if (regionResponse.Contains("Oceania")) return "Oceania";
            return regionResponse;
        }
    }
}
