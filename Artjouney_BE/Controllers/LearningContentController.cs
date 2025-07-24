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
        [RequestSizeLimit(50 * 1024 * 1024)]
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

        [HttpPost("/api/quiz/learning-content/{learningContentId}/user/{userId}")]
        public async Task<IActionResult> StartQuizAsync(long learningContentId, long userId)
        {
            var responseData = await _learningContentService.StartQuizAsync(userId, learningContentId);
            return StatusCode(responseData.Code, responseData);
        }

        [HttpPost("/api/quiz/submit")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SubmitQuizAsync(SubmitQuizRequestDTO submitQuizRequest)
        {
            var responseData = await _learningContentService.SubmitQuizAsync(submitQuizRequest);
            return StatusCode(responseData.Code, responseData);
        }

        [HttpPost("/api/quizs")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateQuizTitle(CreateQuizTitleRequestDTO createQuizTitleRequestDTO)
        {
            var responseData = await _learningContentService.CreateQuizTitle(createQuizTitleRequestDTO);
            return StatusCode(responseData.Code,responseData);
        }

        [HttpDelete("/api/learning-contents/{learningContentId}")]
        public async Task<IActionResult> RemoveLearningContent(long learningContentId)
        {
            var response = await _learningContentService.SoftDeleteLearningContentAsync(learningContentId);
            return StatusCode(response.Code, response);
        }
    }
}
