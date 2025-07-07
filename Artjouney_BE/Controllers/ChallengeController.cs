using BusinessObjects.Models;
using Helpers.DTOs.Challenge;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChallengeController : ControllerBase
    {
        private readonly IChallengeService _challengeService;

        public ChallengeController(IChallengeService challengeService)
        {
            _challengeService = challengeService;
        }

        [HttpGet("api/challenges/course/{courseId}")]
        public async Task<IActionResult> GetAllChallengesByCourseIdAsync(long courseId)
        {
            var response = await _challengeService.GetAllChallengesByCourseIdAsync(courseId);
            return StatusCode(response.Code, response);
        }

        [HttpPost("api/challenges")]
        public async Task<IActionResult> CreateChallengesAsync([FromBody] List<CreateChallengeBasicRequestDTO> challenges)
        {
            var response = await _challengeService.CreateChallengesAsync(challenges);
            return StatusCode(response.Code, response);
        }

        [HttpGet("/api/challenge/{challengeId}")]
        public async Task<IActionResult> GetChallengeDetailbyChallengeId(long challengeId)
        {
            var response = await _challengeService.GetChallengeDetailbyChallengeId(challengeId);
            return StatusCode(response.Code, response);
        }

        [HttpPost("/api/challenge/artworks")]
        public async Task<IActionResult> CreateArtworks([FromBody] List<ArtworkCreateRequestDTO> requestDTOs)
        {
            var response = await _challengeService.CreateArtworks(requestDTOs);
            return StatusCode(response.Code, response);
        }

        [HttpPost("/api/challenge/artwork-details")]
        public async Task<IActionResult> CreateArtworks([FromBody] List<ArtworkDetailCreateRequestDTO> requestDTOs)
        {
            var response = await _challengeService.CreateArtworkDetails(requestDTOs);
            return StatusCode(response.Code, response);
        }
    }
}
