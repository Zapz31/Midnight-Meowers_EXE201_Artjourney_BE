using BusinessObjects.Models;
using Helpers.DTOs.Challenge;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IChallengeService
    {
        public Task<ApiResponse<List<Challenge>>> GetAllChallengesByCourseIdAsync(long courseId);
        public Task<ApiResponse<bool>> CreateChallengesAsync(List<CreateChallengeBasicRequestDTO> challenges);
        public Task<ApiResponse<DragDropChallengeDetailResponse>> GetChallengeDetailbyChallengeId(long challengeId);
        public Task<ApiResponse<bool>> CreateArtworks(List<ArtworkCreateRequestDTO> requestDTOs);
        public Task<ApiResponse<bool>> CreateArtworkDetails(List<ArtworkDetailCreateRequestDTO> requestDTOs);
        public Task<ApiResponse<List<ArtworkViewBasicResponseDTO>>> GetArtworksByChallengeIdAsync(long challengeId);
        public Task<ApiResponse<string>> SaveGameSession(SaveGameSessionRequestDTO saveGameSessionRequestDTO);
        public Task<ApiResponse<ChallengeLeaderboardResponseDTO>> GetChallengeLeaderboard(long challengeId);
        public Task<ApiResponse<Artwork>> CreateSingleArtwork(ArtworkCreateSingleRequestDTO requestDTO);
    }
}
