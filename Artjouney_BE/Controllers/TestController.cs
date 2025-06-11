using BusinessObjects.Enums;
using Helpers.DTOs.Courses;
using Helpers.DTOs.FileHandler;
using Helpers.HelperClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IFileHandlerService _fileHandlerService;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;

        public TestController (IFileHandlerService fileHandlerService, 
            ICourseRepository courseRepository, 
            ICourseService courseService,
            IUserService userService
            )
        {
            _fileHandlerService = fileHandlerService;
            _courseRepository = courseRepository;
            _courseService = courseService;
            _userService = userService;
        }
        [HttpGet("ping")]
        public async Task<IActionResult> ping()
        {
            return Ok("pong");
        }

        [HttpPost("test-file-upload")]
        public async Task<IActionResult> TestFileUpload([FromForm] CourseDTO request)
        {
            ApiResponse<UploadResult> apiResponse = new ApiResponse<UploadResult>();
            UploadResult r = new();
            if (request.Videos != null)
            {
                UploadResult result = await _fileHandlerService.UploadFiles(request.Videos, request.Title, "videos");
                r = result;
            }
            if (request.CourseImages != null)
            {
                UploadResult resultimg = await _fileHandlerService.UploadFiles(request.CourseImages, request.Title, "images");
                //r = resultimg;
            }
            
            
            apiResponse.Status = ResponseStatus.Success;
            apiResponse.Code = 201;
            apiResponse.Data = r;
            return StatusCode(apiResponse.Code, apiResponse);
        }

        [HttpGet("/get-total-learningcontent-and-timelimit-of-a-course")]
        public async Task<IActionResult> GetTotalTimeLimitAndLearningContent([FromQuery] long courseId)
        {
            var data = await _courseRepository.GetCourseLearningStatisticsOptimizedAsync(courseId);
            return StatusCode(200, data);
        }

        [HttpPost("/create-list-userprogress/{courseId}/user/{userId}")]
        public async Task<IActionResult> CreateLearningProgressesByUserIdAndCourseId(long courseId, long userId)
        {
            var data = await _userService.CreateUserLearningProgressesByUserIdAndLNId(courseId, userId);
            return StatusCode(200, data);
        }

    }
}
