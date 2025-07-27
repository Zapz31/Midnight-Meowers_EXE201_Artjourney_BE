using Helpers.DTOs.Courses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseRecommendationController : ControllerBase
    {
        private readonly ICourseRecommendationService _courseRecommendationService;
        private readonly ICurrentUserService _currentUserService;

        public CourseRecommendationController(
            ICourseRecommendationService courseRecommendationService,
            ICurrentUserService currentUserService)
        {
            _courseRecommendationService = courseRecommendationService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Get personalized course recommendations based on user's survey responses and enrollment history
        /// </summary>
        /// <returns>List of personalized course recommendations</returns>
        [HttpGet("personalized")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetPersonalizedRecommendations()
        {
            var userId = _currentUserService.AccountId;
            var response = await _courseRecommendationService.GetPersonalizedRecommendationsAsync(userId);
            return StatusCode(response.Code, response);
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
            var response = await _courseRecommendationService.GetSurveyBasedRecommendationsAsync(userId);
            return StatusCode(response.Code, response);
        }

        /// <summary>
        /// Get course recommendations based on user's enrollment history
        /// </summary>
        /// <returns>List of enrollment-based course recommendations</returns>
        [HttpGet("enrollment-based")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetEnrollmentBasedRecommendations()
        {
            var userId = _currentUserService.AccountId;
            var response = await _courseRecommendationService.GetEnrollmentBasedRecommendationsAsync(userId);
            return StatusCode(response.Code, response);
        }

        /// <summary>
        /// Get trending courses based on enrollment patterns
        /// </summary>
        /// <returns>List of trending courses</returns>
        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingCourses()
        {
            var response = await _courseRecommendationService.GetTrendingCoursesAsync();
            return StatusCode(response.Code, response);
        }

        /// <summary>
        /// Get beginner-friendly courses for new users
        /// </summary>
        /// <returns>List of beginner-level courses</returns>
        [HttpGet("beginner")]
        public async Task<IActionResult> GetBeginnerRecommendations()
        {
            var response = await _courseRecommendationService.GetBeginnerRecommendationsAsync();
            return StatusCode(response.Code, response);
        }

        /// <summary>
        /// Get personalized recommendations for a specific user (Admin only)
        /// </summary>
        /// <param name="userId">User ID to get recommendations for</param>
        /// <returns>List of personalized course recommendations</returns>
        [HttpGet("user/{userId}/personalized")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetPersonalizedRecommendationsForUser([Required] long userId)
        {
            var response = await _courseRecommendationService.GetPersonalizedRecommendationsAsync(userId);
            return StatusCode(response.Code, response);
        }

        /// <summary>
        /// Get survey-based recommendations for a specific user (Admin only)
        /// </summary>
        /// <param name="userId">User ID to get recommendations for</param>
        /// <returns>List of survey-based course recommendations</returns>
        [HttpGet("user/{userId}/survey-based")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetSurveyBasedRecommendationsForUser([Required] long userId)
        {
            var response = await _courseRecommendationService.GetSurveyBasedRecommendationsAsync(userId);
            return StatusCode(response.Code, response);
        }

        /// <summary>
        /// Get enrollment-based recommendations for a specific user (Admin only)
        /// </summary>
        /// <param name="userId">User ID to get recommendations for</param>
        /// <returns>List of enrollment-based course recommendations</returns>
        [HttpGet("user/{userId}/enrollment-based")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetEnrollmentBasedRecommendationsForUser([Required] long userId)
        {
            var response = await _courseRecommendationService.GetEnrollmentBasedRecommendationsAsync(userId);
            return StatusCode(response.Code, response);
        }
    }
}
