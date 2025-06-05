using BusinessObjects.Models;
using Helpers.HelperClasses;
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

        [HttpPost("/users/log-course-access")]
        public async Task<IActionResult> LogCourseAccessAsync([FromBody] UserCourseStreak courseStreak)
        {
            ApiResponse<bool> response = await _userService.LogCourseAccessAsync(courseStreak);
            return StatusCode(response.Code, response);
        }
    }
}
