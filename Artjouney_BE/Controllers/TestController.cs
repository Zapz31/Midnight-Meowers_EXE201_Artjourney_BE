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
        private readonly ISubModuleRepository _subModuleRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly IUserRepository _userRepository;

        public TestController (IFileHandlerService fileHandlerService, 
            ICourseRepository courseRepository, 
            ICourseService courseService,
            IUserService userService,
            ISubModuleRepository subModuleRepository,
            IModuleRepository moduleRepository,
            IUserRepository userRepository
            )
        {
            _fileHandlerService = fileHandlerService;
            _courseRepository = courseRepository;
            _courseService = courseService;
            _userService = userService;
            _subModuleRepository = subModuleRepository;
            _moduleRepository = moduleRepository;
            _userRepository = userRepository;
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

        [HttpPost("/tracking-to-update-sub-module-progress/user/{userId}/sub-module-id/{subModuleId}")]
        public async Task<IActionResult> UpdateSubModuleProgress(long subModuleId, long userId)
        {
            var roweffect = await _subModuleRepository.UpdateSubModuleProgress(userId, subModuleId);
            return StatusCode(200, roweffect);
        }

        [HttpPost("/tracking-to-update-module-progress/user/{userId}/module-id/{moduleId}")]
        public async Task<IActionResult> UpdateModuleProgress(long userId, long moduleId)
        {
            var roweffect = await _moduleRepository.UpdateModuleProgress(userId, moduleId);
            return StatusCode(200, roweffect);
        }

        [HttpPost("/tracking-to-update-course-progress/user/{userId}/course/{courseId}")]
        public async Task<IActionResult> U(long userId, long courseId)
        {
            var roweffect = await _userRepository.UpdateCourseProgress(userId, courseId);
            return StatusCode(200, roweffect);
        }
    }
}
