using Helpers.DTOs.Authentication;
using Helpers.DTOs.UserCourseInfo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserCourseInfoController : ControllerBase
    {
        private readonly IUserCourseInfoService _userCourseInfoService;
        public UserCourseInfoController(IUserCourseInfoService userCourseInfoService)
        {
            _userCourseInfoService = userCourseInfoService;
        }

        [HttpPost("/api/user-course-infos")]
        public async Task<IActionResult> CreateUserCourseInfoAsync([FromBody] BasicCreateUserCourseInfoRequestDTO requestDTO)
        {
            var response = await _userCourseInfoService.CreateUserCourseInfo(requestDTO);
            return StatusCode(response.Code, response);
        }

        [HttpGet("/api/user-course-infos/userId{userId}/courseId/{courseId}")]
        public async Task<IActionResult> GetUserCourseInfoByUserIdAndCourseId(long userId, long courseId)
        {
            var response = await _userCourseInfoService.GetUserCourseInfo(userId, courseId);
            return StatusCode(response.Code, response);
        }
    }
}
