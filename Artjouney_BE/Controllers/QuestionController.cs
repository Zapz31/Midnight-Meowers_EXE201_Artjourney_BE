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
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        [HttpPost("/api/questions")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateQuestionsQuiz([FromBody] List<CreateQuestionsAndOptionsBasicRequestDTO> CreateQuestionsAndOptionsBasicRequestDTOs)
        {
            var responseData = await _questionService.CreateQuestionsAndOptionsAsync(CreateQuestionsAndOptionsBasicRequestDTOs);
            return StatusCode(responseData.Code, responseData);
        }
    }
}
