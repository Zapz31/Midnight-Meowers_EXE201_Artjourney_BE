using BusinessObjects.Enums;
using Helpers.DTOs.Courses;
using Helpers.DTOs.FileHandler;
using Helpers.HelperClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IFileHandlerService _fileHandlerService;

        public TestController (IFileHandlerService fileHandlerService)
        {
            _fileHandlerService = fileHandlerService;
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
    }
}
