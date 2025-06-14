using Helpers.DTOs.CourseReivew;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseReviewController : ControllerBase
    {
        private readonly ICourseReviewService _courseReviewService;
        private readonly ICurrentUserService _currentUserService;
        public CourseReviewController(
            ICourseReviewService courseReviewService, 
            ICurrentUserService currentUserService
            )
        {
            _courseReviewService = courseReviewService;
            _currentUserService = currentUserService;
        }

        [HttpPost("/api/course-reviews")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateCourseReview([FromBody] CreateCourseReviewRequestDTO createCourseReviewRequestDTO)
        {
            var user_id = _currentUserService.AccountId;
            var status = _currentUserService.Status;
            var response = await _courseReviewService.CreateCourseReview(createCourseReviewRequestDTO, user_id, status);
            return StatusCode(response.Code, response);
        }

        [HttpGet("/api/course-reviews/course/{courseId}")]
        public async Task<IActionResult> GGetBasicCourseReviewFlatResponseDTOs(long courseId)
        {
            var response = await _courseReviewService.GetBasicCourseReviewFlatResponseDTOs(courseId);
            return StatusCode(response.Code, response);
        }
    }
}
