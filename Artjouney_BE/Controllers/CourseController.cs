using BusinessObjects.Models;
using Helpers.DTOs.Authentication;
using Helpers.DTOs.Courses;
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
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpPost("/api/courses")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateCourse([FromForm] CourseDTO registerDto)
        {
            //Console.WriteLine($"level value after parse: {registerDto.Level.ToString()}");
            //return Ok(registerDto);
            ApiResponse<Course> response = await _courseService.CreateCourse(registerDto);
            return StatusCode(response.Code, response);
        }

        [HttpGet("/api/courses")]
        public async Task<IActionResult> CreateCourse()
        {
            ApiResponse<List<CourseDTO>> response = await _courseService.GetAllPublishedCoursesAsync();
            return StatusCode(response.Code, response);
        }
    }
}
