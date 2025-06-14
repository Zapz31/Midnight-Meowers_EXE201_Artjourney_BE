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

        [HttpPost("/api/learning-contents")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateLearningContentAsync([FromForm] CreateLNReadingDTO requestDto)
        {
            var response = await _learningContentService.CreateLNContentReadingAsync(requestDto);
            return StatusCode(response.Code, response);
        }

        [HttpPost("/api/learning-contents/sub-module/{subModuleId}")]
        
        public async Task<IActionResult> GetLearningContentsBySubModuleId([FromForm] CreateLNReadingDTO requestDto)
        {
            var response = await _learningContentService.CreateLNContentReadingAsync(requestDto);
            return StatusCode(response.Code, response);
        }

        [HttpGet("/api/learning-contents/sub-module/{subModuleId}")]

        public async Task<IActionResult> GetLearningContentsBySubModuleId(long subModuleId)
        {
            var response = await _learningContentService.GetLearningContentsBySubmoduleId(subModuleId);
            return StatusCode(response.Code, response);
        }

        [HttpGet("/api/learning-contents/{learningContentId}/challenge-items")]

        public async Task<IActionResult> GetChallengeItemsByLNCId(long learningContentId)
        {
            var response = await _learningContentService.GetChallengeItemsByLNCId(learningContentId);
            return StatusCode(response.Code, response);
        }


    }
}
