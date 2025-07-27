using Helpers.DTOs.Courses;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ICourseRecommendationService
    {
        /// <summary>
        /// Get personalized course recommendations based on user's survey responses
        /// </summary>
        /// <param name="userId">User ID to get recommendations for</param>
        /// <returns>List of recommended courses with reasoning</returns>
        Task<ApiResponse<List<CourseRecommendationDTO>>> GetSurveyBasedRecommendationsAsync(long userId);

        /// <summary>
        /// Get course recommendations based on user's enrollment history
        /// </summary>
        /// <param name="userId">User ID to get recommendations for</param>
        /// <returns>List of recommended courses similar to enrolled courses</returns>
        Task<ApiResponse<List<CourseRecommendationDTO>>> GetEnrollmentBasedRecommendationsAsync(long userId);

        /// <summary>
        /// Get combined recommendations from both survey and enrollment data
        /// </summary>
        /// <param name="userId">User ID to get recommendations for</param>
        /// <returns>List of personalized course recommendations</returns>
        Task<ApiResponse<List<CourseRecommendationDTO>>> GetPersonalizedRecommendationsAsync(long userId);

        /// <summary>
        /// Get trending courses based on enrollment patterns
        /// </summary>
        /// <param name="userId">Optional user ID to exclude enrolled courses</param>
        /// <returns>List of popular/trending courses</returns>
        Task<ApiResponse<List<CourseRecommendationDTO>>> GetTrendingCoursesAsync(long? userId = null);

        /// <summary>
        /// Get beginner-friendly courses for new users
        /// </summary>
        /// <returns>List of beginner-level courses</returns>
        Task<ApiResponse<List<CourseRecommendationDTO>>> GetBeginnerRecommendationsAsync();
    }
}
