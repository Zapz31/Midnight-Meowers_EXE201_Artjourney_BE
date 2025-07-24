using BusinessObjects.Models;
using DAOs;
using Helpers.DTOs.Challenge;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChallengeController : ControllerBase
    {
        private readonly IChallengeService _challengeService;
        private readonly ApplicationDbContext _context;

        public ChallengeController(IChallengeService challengeService, ApplicationDbContext applicationDbContext)
        {
            _challengeService = challengeService;
            _context = applicationDbContext;
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

        [HttpGet("/api/challenge/{challengeId}/artworks")]
        public async Task<IActionResult> GetArtworksByChallengeId(long challengeId)
        {
            var response = await _challengeService.GetArtworksByChallengeIdAsync(challengeId);
            return StatusCode(response.Code, response);
        }

        [HttpPost("/api/game/sessions")] //http://localhost:8000/api/game/sessions
        public async Task<IActionResult> SaveGameSession([FromBody] SaveGameSessionRequestDTO saveGameSessionRequestDTO)
        {
            var response = await _challengeService.SaveGameSession(saveGameSessionRequestDTO);
            return StatusCode(response.Code, response);
        }

        [HttpGet("/api/challenge/leaderboard/{challengeId}")]
        public async Task<IActionResult> GetChallengeLeaderboard(long challengeId)
        {
            var response = await _challengeService.GetChallengeLeaderboard(challengeId);
            return StatusCode(response.Code, response);
        }

        [HttpGet("/api/leaderboard/global")]
        public async Task<IActionResult> GetGlobalLeaderboard()
        {
            try
            {
                var leaderboardData = await _context.UserChallengeHighestScores
                    .Include(u => u.User)
                    .GroupBy(x => new { x.UserId, x.User.Fullname, x.User.AvatarUrl })
                    .Select(g => new
                    {
                        UserId = g.Key.UserId,
                        Username = g.Key.Fullname ?? "Unknown",
                        AvatarUrl = g.Key.AvatarUrl,
                        TotalScore = g.Sum(x => x.HighestScore),
                        ChallengesCompleted = g.Count()
                    })
                    .OrderByDescending(x => x.TotalScore)
                    .ToListAsync();

                if (leaderboardData == null || leaderboardData.Count == 0)
                {
                    return NotFound(new { error = "No attempts recorded" });
                }

                var leaderboard = leaderboardData
                    .Select((record, index) => new
                    {
                        Rank = index + 1,
                        record.UserId,
                        record.Username,
                        record.AvatarUrl,
                        record.TotalScore,
                        record.ChallengesCompleted
                    });

                return Ok(new { leaderboard });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching global leaderboard: {ex.Message}");
                return StatusCode(500, new { error = "Failed to retrieve leaderboard data" });
            }
        }

    }
}
