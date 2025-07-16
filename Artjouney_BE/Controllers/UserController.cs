using BusinessObjects.Models;
using Helpers.DTOs.UserLearningProgress;
using Helpers.HelperClasses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("/api/users/log-course-access")]
        public async Task<IActionResult> LogCourseAccessAsync([FromBody] UserCourseStreak courseStreak)
        {
            ApiResponse<bool> response = await _userService.LogCourseAccessAsync(courseStreak);
            return StatusCode(response.Code, response);
        }

        [HttpPost("/api/users/user-learning-progress")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateUserLearningProgress([FromBody] CreateULPRequestDTO createULPRequestDTO)
        {
            ApiResponse<bool> response = await _userService.CreateUserLearningProgress(createULPRequestDTO);
            return StatusCode(response.Code, response);
        }

        [HttpPost("/api/users/mark-as-complete/learning-content/{userLearningContentId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> MarkAsCompleteUserLearningProgressSingleAsync(long userLearningContentId)
        {
            ApiResponse<UserLearningProgress> response = await _userService.MarkAsCompleteUserLearningProgressSingleAsync(userLearningContentId);
            return StatusCode(response.Code, response);
        }

        [HttpGet("/api/users/premium-status")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetLatestPremiumInfoByUserIdAsync()
        {
            var resposne = await _userService.GetLatestPremiumInfoByUserIdAsync();
            return StatusCode(resposne.Code, resposne);
        }
    }
}
