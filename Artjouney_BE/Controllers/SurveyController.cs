using Helpers.DTOs.Survey;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SurveyController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        public SurveyController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        // ===== ADMIN ENDPOINTS =====

        /// <summary>
        /// Create a single survey question with options (Admin only)
        /// </summary>
        [HttpPost("admin/questions")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateSurveyQuestion([FromBody] CreateSurveyRequestDTO createSurveyRequest)
        {
            var response = await _surveyService.CreateSurveyQuestionAsync(createSurveyRequest);
            return StatusCode(response.Code, response);
        }

        /// <summary>
        /// Create multiple survey questions with options (Admin only)
        /// </summary>
        [HttpPost("admin/questions/bulk")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateMultipleSurveyQuestions([FromBody] CreateMultipleSurveysRequestDTO createMultipleSurveysRequest)
        {
            var response = await _surveyService.CreateMultipleSurveyQuestionsAsync(createMultipleSurveysRequest);
            return StatusCode(response.Code, response);
        }

        /// <summary>
        /// Update a survey question and its options (Admin only)
        /// </summary>
        [HttpPut("admin/questions/{questionId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateSurveyQuestion(long questionId, [FromBody] UpdateSurveyRequestDTO updateSurveyRequest)
        {
            updateSurveyRequest.SurveyQuestionId = questionId;
            var response = await _surveyService.UpdateSurveyQuestionAsync(updateSurveyRequest);
            return StatusCode(response.Code, response);
        }

        /// <summary>
        /// Delete a survey question (Admin only)
        /// </summary>
        [HttpDelete("admin/questions/{questionId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteSurveyQuestion(long questionId)
        {
            var response = await _surveyService.DeleteSurveyQuestionAsync(questionId);
            return StatusCode(response.Code, response);
        }

        /// <summary>
        /// Get all survey questions with options (Admin only)
        /// </summary>
        [HttpGet("admin/questions")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAllSurveyQuestions()
        {
            var response = await _surveyService.GetAllSurveyQuestionsAsync();
            return StatusCode(response.Code, response);
        }

        /// <summary>
        /// Get a specific survey question by ID (Admin only)
        /// </summary>
        [HttpGet("admin/questions/{questionId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetSurveyQuestionById(long questionId)
        {
            var response = await _surveyService.GetSurveyQuestionByIdAsync(questionId);
            return StatusCode(response.Code, response);
        }

        // ===== USER ENDPOINTS =====

        /// <summary>
        /// Get active survey questions for users to answer
        /// </summary>
        [HttpGet("questions")]
                [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> GetActiveSurveyQuestions()
        {
            var response = await _surveyService.GetActiveSurveyQuestionsSimpleAsync();
            return StatusCode(response.Code, response);
        }

        /// <summary>
        /// Submit user's survey answers
        /// </summary>
        [HttpPost("submit")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SubmitUserSurvey([FromBody] UserSurveySubmissionDTO userSurveySubmission)
        {
            var response = await _surveyService.SubmitUserSurveyAsync(userSurveySubmission);
            return StatusCode(response.Code, response);
        }

        /// <summary>
        /// Get user's survey history (what they answered)
        /// </summary>
        [HttpGet("my-survey")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUserSurveyHistory()
        {
            var response = await _surveyService.GetUserSurveyHistoryAsync();
            return StatusCode(response.Code, response);
        }

        /// <summary>
        /// Check if user has completed the survey
        /// </summary>
        [HttpGet("status")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CheckUserSurveyStatus()
        {
            var response = await _surveyService.CheckUserSurveyStatusAsync();
            return StatusCode(response.Code, response);
        }
    }
}
