using Helpers.DTOs.Courses;
using Helpers.HelperClasses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly ICourseRecommendationService _recommendationService;
        private readonly ICurrentUserService _currentUserService;

        public RecommendationController(
            ICourseRecommendationService recommendationService,
            ICurrentUserService currentUserService)
        {
            _recommendationService = recommendationService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Get personalized course recommendations for the current user
        /// Combines survey responses and enrollment history for best recommendations
        /// </summary>
        /// <returns>List of personalized course recommendations</returns>
        [HttpGet("personalized")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetPersonalizedRecommendations()
        {
            var userId = _currentUserService.AccountId;
            var result = await _recommendationService.GetPersonalizedRecommendationsAsync(userId);
            
            return result.Code switch
            {
                200 => Ok(result),
                500 => StatusCode(500, result),
                _ => StatusCode(500, result)
            };
        }

        /// <summary>
        /// Get course recommendations based on user's survey responses only
        /// </summary>
        /// <returns>List of survey-based course recommendations</returns>
        [HttpGet("survey-based")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetSurveyBasedRecommendations()
        {
            var userId = _currentUserService.AccountId;
            var result = await _recommendationService.GetSurveyBasedRecommendationsAsync(userId);
            
            return result.Code switch
            {
                200 => Ok(result),
                500 => StatusCode(500, result),
                _ => StatusCode(500, result)
            };
        }

        /// <summary>
        /// Get course recommendations based on user's enrollment history only
        /// </summary>
        /// <returns>List of enrollment-based course recommendations</returns>
        [HttpGet("enrollment-based")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetEnrollmentBasedRecommendations()
        {
            var userId = _currentUserService.AccountId;
            var result = await _recommendationService.GetEnrollmentBasedRecommendationsAsync(userId);
            
            return result.Code switch
            {
                200 => Ok(result),
                500 => StatusCode(500, result),
                _ => StatusCode(500, result)
            };
        }

        /// <summary>
        /// Get trending courses (popular and highly-rated)
        /// Available for all users, no authentication required
        /// </summary>
        /// <returns>List of trending courses</returns>
        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingCourses()
        {
            var result = await _recommendationService.GetTrendingCoursesAsync();
            
            return result.Code switch
            {
                200 => Ok(result),
                500 => StatusCode(500, result),
                _ => StatusCode(500, result)
            };
        }

        /// <summary>
        /// Get beginner-friendly courses
        /// Available for all users, no authentication required
        /// </summary>
        /// <returns>List of beginner courses</returns>
        [HttpGet("beginner")]
        public async Task<IActionResult> GetBeginnerRecommendations()
        {
            var result = await _recommendationService.GetBeginnerRecommendationsAsync();
            
            return result.Code switch
            {
                200 => Ok(result),
                500 => StatusCode(500, result),
                _ => StatusCode(500, result)
            };
        }

        /// <summary>
        /// Get personalized recommendations for a specific user (Admin only)
        /// </summary>
        /// <param name="userId">User ID to get recommendations for</param>
        /// <returns>List of personalized course recommendations</returns>
        [HttpGet("user/{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUserRecommendations([Required] long userId)
        {
            // Check if current user is admin or requesting their own recommendations
            var currentUserId = _currentUserService.AccountId;
            var currentUserRole = _currentUserService.Role;
            
            if (currentUserId != userId && !"Admin".Equals(currentUserRole))
            {
                return Forbid("You can only access your own recommendations unless you're an admin");
            }

            var result = await _recommendationService.GetPersonalizedRecommendationsAsync(userId);
            
            return result.Code switch
            {
                200 => Ok(result),
                500 => StatusCode(500, result),
                _ => StatusCode(500, result)
            };
        }
    }
}
