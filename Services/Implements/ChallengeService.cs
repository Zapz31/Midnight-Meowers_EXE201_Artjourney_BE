using Azure.Core;
using Bogus;
using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.Challenge;
using Helpers.HelperClasses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implements
{
    public class ChallengeService : IChallengeService
    {
        private readonly ILogger<ChallengeService> _logger;
        private readonly IChallengeRepository _challengeRepository;
        private readonly IArtworkRepository _artworkRepository;
        private readonly IArtworkDetailRepository _artworkDetailRepository;
        private readonly IUserRepository _userRepository;
        private readonly IChallengeSessionRepository _challengeSessionRepository;
        private readonly IUserChallengeHighestScoreRepository _userChallengeHighestScoreRepository;

        public ChallengeService(ILogger<ChallengeService> logger, 
            IChallengeRepository challengeRepository,
            IArtworkRepository artworkRepository,
            IArtworkDetailRepository artworkDetailRepository,
            IUserRepository userRepository,
            IChallengeSessionRepository challengeSessionRepository,
            IUserChallengeHighestScoreRepository userChallengeHighestScoreRepository)
        {
            _logger = logger;
            _challengeRepository = challengeRepository;
            _artworkRepository = artworkRepository;
            _artworkDetailRepository = artworkDetailRepository;
            _userRepository = userRepository;
            _challengeSessionRepository = challengeSessionRepository;
            _userChallengeHighestScoreRepository = userChallengeHighestScoreRepository;
        }

        public async Task<ApiResponse<List<Challenge>>> GetAllChallengesByCourseIdAsync(long courseId)
        {
            try
            {
                var responesData = await _challengeRepository.GetAllChallengesByCourseIdAsync(courseId);
                return new ApiResponse<List<Challenge>>
                {
                    Code = 200,
                    Status = ResponseStatus.Success,
                    Data = responesData,
                    Message = "Data retrieved successfully"
                };
            } catch (Exception ex)
            {
                _logger.LogError("Error at GetAllChallengesByCourseIdAsync in ChallengeService: {ex}", ex.Message);
                return new ApiResponse<List<Challenge>> 
                {
                    Code = 500,
                    Status = ResponseStatus.Error,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>> CreateChallengesAsync(List<CreateChallengeBasicRequestDTO> challenges)
        {
            try
            {
                List<Challenge> createChallenges = new List<Challenge>();
                foreach (var challenge in challenges)
                {
                    var createChallenge = new Challenge()
                    {
                        Name = challenge.Name,
                        CourseId = challenge.CourseId,
                        Description = challenge.Description,
                        ChallengeType = challenge.ChallengeType,
                        DurationSeconds = challenge.DurationSeconds,
                    };
                    createChallenges.Add(createChallenge);
                }
                await _challengeRepository.CreateChallengesAsync(createChallenges);
                return new ApiResponse<bool>
                {
                    Code = 201,
                    Status = ResponseStatus.Success,
                    Data = true
                };
            } catch (Exception ex)
            {
                _logger.LogError("Error at CreateChallengesAsync in ChallengeService: {ex}", ex.Message);
                return new ApiResponse<bool>
                {
                    Code = 500,
                    Status = ResponseStatus.Error,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<DragDropChallengeDetailResponse>> GetChallengeDetailbyChallengeId(long challengeId)
        {
            try
            {
                // get challenge
                var challenge = await _challengeRepository.GetChallengeByIdAsync(challengeId);
                if (challenge == null)
                {
                    return new ApiResponse<DragDropChallengeDetailResponse>
                    {
                        Code = 400,
                        Status = ResponseStatus.Error,
                        Message = $"Challenge with ID ${challengeId} not found"
                    };
                }

                var challengeResponse = new ChallengeViewAResponseDTO
                {
                    Id = challenge.Id,
                    Name = challenge.Name,
                    DurationSeconds = (int)challenge.DurationSeconds,
                };

                // get artworks
                var artworks = await _artworkRepository.GetAllArtworksByChallengeIdAsync(challengeId);
                if (artworks.Count < 1 || artworks == null)
                {
                    return new ApiResponse<DragDropChallengeDetailResponse>
                    {
                        Code = 400,
                        Status = ResponseStatus.Error,
                        Message = $"No game data found for challenge ID ${challengeId}"
                    };
                }

                var artworkIds = artworks.Select(a => a.Id).ToList();

                // get artwork details
                var details = await _artworkDetailRepository.GetArtworkDetailByArtworkIds(artworkIds);

                var responseData = new DragDropChallengeDetailResponse
                {
                    Challenge = challengeResponse,
                    Artworks = artworks,
                    Details = details
                };
                return new ApiResponse<DragDropChallengeDetailResponse>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = responseData,
                    Message = "OK"
                };
            } catch (Exception ex)
            {
                _logger.LogError("Error at GetChallengeDetailbyChallengeId in ChallengeService: {ex}", ex.Message);
                return new ApiResponse<DragDropChallengeDetailResponse>
                {
                    Code = 500,
                    Status = ResponseStatus.Error,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>> CreateArtworks(List<ArtworkCreateRequestDTO> requestDTOs)
        {
            try
            {
                List<Artwork> createArtworks = new();
                foreach (var dto in requestDTOs)
                {
                    var artwork = new Artwork 
                    {
                        Image = dto.Image,
                        Title = dto.Title,
                        ChallengeId = dto.ChallengeId,
                    };
                    createArtworks.Add(artwork);
                }
                await _artworkRepository.CreateArtworks(createArtworks);
                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = true,
                };
                
            } catch(Exception ex) 
            {
                _logger.LogError("Error at GetChallengeDetailbyChallengeId in ChallengeService: {ex}", ex.Message);
                return new ApiResponse<bool>
                {
                    Code = 500,
                    Status = ResponseStatus.Error,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>> CreateArtworkDetails(List<ArtworkDetailCreateRequestDTO> requestDTOs)
        {
            try
            {
                List<ArtworkDetail> createArtworkDetails = new();
                foreach (var dto in requestDTOs)
                {
                    var artworkDetail = new ArtworkDetail
                    {
                        Artist = dto.Artist,
                        Period = dto.Period,
                        Year = dto.Year,
                        ArtworkId = dto.ArtworkId
                    };
                    createArtworkDetails.Add(artworkDetail);
                }
                await _artworkDetailRepository.CreateArtworkDetails(createArtworkDetails);
                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = true,
                };

            }
            catch (Exception ex)
            {
                _logger.LogError("Error at GetChallengeDetailbyChallengeId in ChallengeService: {ex}", ex.Message);
                return new ApiResponse<bool>
                {
                    Code = 500,
                    Status = ResponseStatus.Error,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<string>> SaveGameSession(SaveGameSessionRequestDTO saveGameSessionRequestDTO)
        {
            try
            {
                // Validate user existence
                var user = await _userRepository.GetUserByIDAsync(saveGameSessionRequestDTO.UserId);
                if (user == null)
                {
                    return new ApiResponse<string>
                    {
                        Status = ResponseStatus.Error,
                        Code = 400,
                        Message = "User not exist"
                    };
                }

                // Validate challenge existence
                var challenge = await _challengeRepository.GetChallengeByIdAsync(saveGameSessionRequestDTO.ChallengeId);
                if(challenge == null)
                {
                    return new ApiResponse<string>
                    {
                        Status = ResponseStatus.Error,
                        Code = 400,
                        Message = "Challenge not exist"
                    };
                }

                // Create new session
                var session = new ChallengeSession
                {
                    UserId = saveGameSessionRequestDTO.UserId,
                    ChallengeId = saveGameSessionRequestDTO.ChallengeId,
                    Score = saveGameSessionRequestDTO.Score,
                    TimeTaken = saveGameSessionRequestDTO.TimeTaken,
                    IsComplete = saveGameSessionRequestDTO.IsCompleted,
                    CreatedAt = DateTime.UtcNow,
                };

                await _challengeSessionRepository.CreateChallengeSessionAsync(session);

                var highestScoreRecord = await _userChallengeHighestScoreRepository.
                    GetHighestScoreByUserIdAndChallengeId(saveGameSessionRequestDTO.UserId, saveGameSessionRequestDTO.ChallengeId);
                if(highestScoreRecord == null)
                {
                    var newHighestScore = new UserChallengeHighestScore
                    {
                        UserId = saveGameSessionRequestDTO.UserId,
                        ChallengeId = saveGameSessionRequestDTO.ChallengeId,
                        HighestScore = saveGameSessionRequestDTO.Score,
                        TimeTaken = saveGameSessionRequestDTO.TimeTaken,
                        AttemptedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };
                    await _userChallengeHighestScoreRepository.CreateUserChallengeHighestScoreAsync(newHighestScore);
                } else if(saveGameSessionRequestDTO.Score > highestScoreRecord.HighestScore ||
                        (saveGameSessionRequestDTO.Score == highestScoreRecord.HighestScore && 
                        saveGameSessionRequestDTO.TimeTaken < highestScoreRecord.TimeTaken))
                {
                    highestScoreRecord.HighestScore = saveGameSessionRequestDTO.Score;
                    highestScoreRecord.TimeTaken = saveGameSessionRequestDTO.TimeTaken;
                    highestScoreRecord.AttemptedAt = DateTime.UtcNow;
                    highestScoreRecord.UpdatedAt = DateTime.UtcNow;
                    await _userChallengeHighestScoreRepository.UpdateUserChallengeHighestScoreAsync(highestScoreRecord);
                }
                return new ApiResponse<string>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = "Updated successfully"
                };

            }
            catch (Exception ex)
            {
                _logger.LogError("Error at GetChallengeDetailbyChallengeId in ChallengeService: {ex}", ex.Message);
                return new ApiResponse<string>
                {
                    Code = 500,
                    Status = ResponseStatus.Error,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<ChallengeLeaderboardResponseDTO>> GetChallengeLeaderboard(long challengeId)
        {
            try
            {
                // Validate challenge existence
                var challenge = await _challengeRepository.GetChallengeByIdAsync(challengeId);
                if (challenge == null)
                {
                    return new ApiResponse<ChallengeLeaderboardResponseDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 400,
                        Message = "Challenge not exist"
                    };
                }

                // Get leaderboard data
                var leaderboard = await _userChallengeHighestScoreRepository.GetChallengeLearboardAsync(challengeId);
                var responseData = new ChallengeLeaderboardResponseDTO
                {
                    ChallengeId = challengeId,
                    ChallengeName = challenge.Name,
                    Leaderboard = leaderboard.Select((record, index) => new LeaderboardAResponseDTO
                    {
                        Rank = index + 1,
                        UserId = record.UserId,
                        Username = record.User.Fullname ?? "Unknown",
                        HighestScore = record.HighestScore,
                        TimeTaken = record.TimeTaken,
                        AttemptedAt = record.AttemptedAt
                    }).ToList()
                };
                return new ApiResponse<ChallengeLeaderboardResponseDTO>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = responseData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at GetChallengeLeaderboard in ChallengeService: {ex}", ex.Message);
                return new ApiResponse<ChallengeLeaderboardResponseDTO>
                {
                    Code = 500,
                    Status = ResponseStatus.Error,
                    Message = ex.Message
                };
            }
        }

    }
}
