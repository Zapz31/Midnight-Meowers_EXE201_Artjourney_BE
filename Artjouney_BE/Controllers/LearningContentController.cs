using Helpers.DTOs.LearningContent;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningContentController : ControllerBase
    {
        private readonly ILearningContentService _learningContentService;
        public LearningContentController(ILearningContentService learningContentService)
        {
            _learningContentService = learningContentService;
        }

        [HttpPost("/learning-contents")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateLearningContentAsync([FromForm] CreateLNReadingDTO requestDto)
        {
            var response = await _learningContentService.CreateLNContentReadingAsync(requestDto);
            return StatusCode(response.Code, response);
        }
    }
}
