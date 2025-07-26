using BusinessObjects.Models;
using BusinessObjects.Enums;
using Helpers.DTOs.UserLearningProgress;
using Helpers.DTOs.Users;
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
        private readonly ICurrentUserService _currentUserService;
        
        public UserController(IUserService userService, ICurrentUserService currentUserService)
        {
            _userService = userService;
            _currentUserService = currentUserService;
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

        [HttpPut("/api/users/profile")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateUserProfileAsync([FromForm] UpdateUserProfileRequestDTO updateProfileRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => new ApiError { Message = e.ErrorMessage })
                    .ToList();

                return BadRequest(new ApiResponse<object>
                {
                    Status = ResponseStatus.Error,
                    Code = 400,
                    Message = "Validation failed",
                    Data = null,
                    Errors = errors
                });
            }

            var response = await _userService.UpdateUserProfileAsync(updateProfileRequest);
            
            if (response.Status == ResponseStatus.Success)
            {
                return Ok(new ApiResponse<object>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Message = "Profile updated successfully",
                    Data = null
                });
            }
            
            return StatusCode(response.Code, new ApiResponse<object>
            {
                Status = response.Status,
                Code = response.Code,
                Message = response.Message,
                Data = null,
                Errors = response.Errors
            });
        }

        [HttpGet("/api/users/profile")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUserProfileAsync()
        {
            try
            {
                var response = await _userService.GetUserByIDAsynce(_currentUserService.AccountId);
                return StatusCode(response.Code, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "Failed to retrieve user profile",
                    Data = null,
                    Errors = new List<ApiError>
                    {
                        new ApiError { Code = 500, Message = ex.Message }
                    }
                });
            }

            
        }
    }
}
