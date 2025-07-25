using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.Survey;
using Helpers.HelperClasses;
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
    public class SurveyService : ISurveyService
    {
        private readonly ILogger<SurveyService> _logger;
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly ISurveyOptionRepository _surveyOptionRepository;
        private readonly IUserOptionChoiceRepository _userOptionChoiceRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;

        public SurveyService(
            ILogger<SurveyService> logger,
            ISurveyQuestionRepository surveyQuestionRepository,
            ISurveyOptionRepository surveyOptionRepository,
            IUserOptionChoiceRepository userOptionChoiceRepository,
            IUserRepository userRepository,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _surveyQuestionRepository = surveyQuestionRepository;
            _surveyOptionRepository = surveyOptionRepository;
            _userOptionChoiceRepository = userOptionChoiceRepository;
            _userRepository = userRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<SurveyQuestionResponseDTO>> CreateSurveyQuestionAsync(CreateSurveyRequestDTO createSurveyRequest)
        {
            try
            {
                var currentUserId = _currentUserService.AccountId;

                // Create survey question
                var surveyQuestion = new SurveyQuestion
                {
                    SurveyQuestionName = createSurveyRequest.SurveyQuestionContent,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId
                };

                var createdQuestion = await _surveyQuestionRepository.CreateSurveyQuestionAsync(surveyQuestion);

                // Create survey options
                var surveyOptions = new List<SurveyOption>();
                foreach (var optionDto in createSurveyRequest.Options)
                {
                    var option = new SurveyOption
                    {
                        SurveyOptionContent = optionDto.SurveyOptionContent,
                        IsAcive = true,
                        CreatedAt = DateTime.UtcNow,
                        SurveyQuestionId = createdQuestion.SurveyQuestionId
                    };
                    surveyOptions.Add(option);
                }

                var createdOptions = await _surveyOptionRepository.CreateMultipleSurveyOptionsAsync(surveyOptions);

                // Get the complete question with options for response
                var questionWithOptions = await _surveyQuestionRepository.GetSurveyQuestionWithOptionsAsync(createdQuestion.SurveyQuestionId);

                var responseDto = MapToSurveyQuestionResponseDTO(questionWithOptions!);

                return new ApiResponse<SurveyQuestionResponseDTO>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = responseDto,
                    Message = "Survey question created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at CreateSurveyQuestionAsync in SurveyService: {ex}", ex.Message);
                return new ApiResponse<SurveyQuestionResponseDTO>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<SurveyQuestionResponseDTO>>> CreateMultipleSurveyQuestionsAsync(CreateMultipleSurveysRequestDTO createMultipleSurveysRequest)
        {
            try
            {
                var createdQuestions = new List<SurveyQuestionResponseDTO>();
                
                foreach (var surveyRequest in createMultipleSurveysRequest.SurveyQuestions)
                {
                    var result = await CreateSurveyQuestionAsync(surveyRequest);
                    if (result.Status == ResponseStatus.Success && result.Data != null)
                    {
                        createdQuestions.Add(result.Data);
                    }
                    else
                    {
                        return new ApiResponse<List<SurveyQuestionResponseDTO>>
                        {
                            Status = ResponseStatus.Error,
                            Code = 400,
                            Message = $"Failed to create survey question: {result.Message}"
                        };
                    }
                }

                return new ApiResponse<List<SurveyQuestionResponseDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = createdQuestions,
                    Message = $"{createdQuestions.Count} survey questions created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at CreateMultipleSurveyQuestionsAsync in SurveyService: {ex}", ex.Message);
                return new ApiResponse<List<SurveyQuestionResponseDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<SurveyQuestionResponseDTO>> UpdateSurveyQuestionAsync(UpdateSurveyRequestDTO updateSurveyRequest)
        {
            try
            {
                // Get existing question
                var existingQuestion = await _surveyQuestionRepository.GetSurveyQuestionWithOptionsAsync(updateSurveyRequest.SurveyQuestionId);
                if (existingQuestion == null)
                {
                    return new ApiResponse<SurveyQuestionResponseDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Message = "Survey question not found"
                    };
                }

                // Update question
                existingQuestion.SurveyQuestionName = updateSurveyRequest.SurveyQuestionContent;
                existingQuestion.IsActive = updateSurveyRequest.IsActive;
                existingQuestion.UpdatedAt = DateTime.UtcNow;

                await _surveyQuestionRepository.UpdateSurveyQuestionAsync(existingQuestion);

                // Handle options
                foreach (var optionDto in updateSurveyRequest.Options)
                {
                    if (optionDto.SurveyOptionId.HasValue)
                    {
                        // Update existing option
                        var existingOption = existingQuestion.SurveyOptions.FirstOrDefault(o => o.SurveyOptionId == optionDto.SurveyOptionId.Value);
                        if (existingOption != null)
                        {
                            if (optionDto.IsDeleted)
                            {
                                await _surveyOptionRepository.DeleteSurveyOptionAsync(existingOption.SurveyOptionId);
                            }
                            else
                            {
                                existingOption.SurveyOptionContent = optionDto.SurveyOptionContent;
                                existingOption.IsAcive = optionDto.IsActive;
                                await _surveyOptionRepository.UpdateSurveyOptionAsync(existingOption);
                            }
                        }
                    }
                    else if (!optionDto.IsDeleted)
                    {
                        // Create new option
                        var newOption = new SurveyOption
                        {
                            SurveyOptionContent = optionDto.SurveyOptionContent,
                            IsAcive = optionDto.IsActive,
                            CreatedAt = DateTime.UtcNow,
                            SurveyQuestionId = updateSurveyRequest.SurveyQuestionId
                        };
                        await _surveyOptionRepository.CreateSurveyOptionAsync(newOption);
                    }
                }

                // Get updated question with options
                var updatedQuestion = await _surveyQuestionRepository.GetSurveyQuestionWithOptionsAsync(updateSurveyRequest.SurveyQuestionId);
                var responseDto = MapToSurveyQuestionResponseDTO(updatedQuestion!);

                return new ApiResponse<SurveyQuestionResponseDTO>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = responseDto,
                    Message = "Survey question updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at UpdateSurveyQuestionAsync in SurveyService: {ex}", ex.Message);
                return new ApiResponse<SurveyQuestionResponseDTO>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteSurveyQuestionAsync(long surveyQuestionId)
        {
            try
            {
                var existingQuestion = await _surveyQuestionRepository.GetSurveyQuestionByIdAsync(surveyQuestionId);
                if (existingQuestion == null)
                {
                    return new ApiResponse<bool>
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Message = "Survey question not found"
                    };
                }

                await _surveyQuestionRepository.DeleteSurveyQuestionAsync(surveyQuestionId);

                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = true,
                    Message = "Survey question deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at DeleteSurveyQuestionAsync in SurveyService: {ex}", ex.Message);
                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<SurveyQuestionResponseDTO>>> GetAllSurveyQuestionsAsync()
        {
            try
            {
                var questions = await _surveyQuestionRepository.GetAllSurveyQuestionsWithOptionsAsync();
                var responseDto = questions.Select(MapToSurveyQuestionResponseDTO).ToList();

                return new ApiResponse<List<SurveyQuestionResponseDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = responseDto,
                    Message = "Survey questions retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at GetAllSurveyQuestionsAsync in SurveyService: {ex}", ex.Message);
                return new ApiResponse<List<SurveyQuestionResponseDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<SurveyQuestionResponseDTO>> GetSurveyQuestionByIdAsync(long surveyQuestionId)
        {
            try
            {
                var question = await _surveyQuestionRepository.GetSurveyQuestionWithOptionsAsync(surveyQuestionId);
                if (question == null)
                {
                    return new ApiResponse<SurveyQuestionResponseDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Message = "Survey question not found"
                    };
                }

                var responseDto = MapToSurveyQuestionResponseDTO(question);

                return new ApiResponse<SurveyQuestionResponseDTO>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = responseDto,
                    Message = "Survey question retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at GetSurveyQuestionByIdAsync in SurveyService: {ex}", ex.Message);
                return new ApiResponse<SurveyQuestionResponseDTO>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<SurveyQuestionResponseDTO>>> GetActiveSurveyQuestionsForUserAsync()
        {
            try
            {
                var questions = await _surveyQuestionRepository.GetAllActiveSurveyQuestionsAsync();
                var responseDto = questions.Select(MapToSurveyQuestionResponseDTO).ToList();

                return new ApiResponse<List<SurveyQuestionResponseDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = responseDto,
                    Message = "Active survey questions retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at GetActiveSurveyQuestionsForUserAsync in SurveyService: {ex}", ex.Message);
                return new ApiResponse<List<SurveyQuestionResponseDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<SimpleSurveyQuestionResponseDTO>>> GetActiveSurveyQuestionsSimpleAsync()
        {
            try
            {
                var questions = await _surveyQuestionRepository.GetAllActiveSurveyQuestionsWithOptionsAsync();
                var responseDto = questions.Select(MapToSimpleSurveyQuestionResponseDTO).ToList();

                return new ApiResponse<List<SimpleSurveyQuestionResponseDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = responseDto,
                    Message = "Active survey questions retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at GetActiveSurveyQuestionsSimpleAsync in SurveyService: {ex}", ex.Message);
                return new ApiResponse<List<SimpleSurveyQuestionResponseDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<string>> SubmitUserSurveyAsync(UserSurveySubmissionDTO userSurveySubmission)
        {
            try
            {
                var currentUserId = _currentUserService.AccountId;

                // Check if user already completed survey
                var hasCompleted = await _userOptionChoiceRepository.HasUserCompletedSurveyAsync(currentUserId);
                if (hasCompleted)
                {
                    // Delete existing choices to allow retaking
                    await _userOptionChoiceRepository.DeleteAllUserChoicesAsync(currentUserId);
                }

                // Create user option choices
                var userChoices = new List<UserOptionChoice>();
                foreach (var answer in userSurveySubmission.Answers)
                {
                    // Validate that the survey option exists and is active
                    var surveyOption = await _surveyOptionRepository.GetSurveyOptionByIdAsync(answer.SurveyOptionId);
                    if (surveyOption == null || !surveyOption.IsAcive)
                    {
                        return new ApiResponse<string>
                        {
                            Status = ResponseStatus.Error,
                            Code = 400,
                            Message = $"Invalid survey option ID: {answer.SurveyOptionId}"
                        };
                    }

                    var userChoice = new UserOptionChoice
                    {
                        UserId = currentUserId,
                        SurveyOptionId = answer.SurveyOptionId,
                        Content = answer.Content,
                        CreatedAt = DateTime.UtcNow
                    };
                    userChoices.Add(userChoice);
                }

                await _userOptionChoiceRepository.CreateMultipleUserOptionChoicesAsync(userChoices);

                // Update user's survey status
                var user = await _userRepository.GetUserByIDAsync(currentUserId);
                if (user != null)
                {
                    user.IsSurveyed = true;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateUserAsync(user);
                }

                return new ApiResponse<string>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = "Survey submitted successfully",
                    Message = "Survey submitted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at SubmitUserSurveyAsync in SurveyService: {ex}", ex.Message);
                return new ApiResponse<string>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<SurveyQuestionResponseDTO>>> GetUserSurveyHistoryAsync()
        {
            try
            {
                var currentUserId = _currentUserService.AccountId;
                var userChoices = await _userOptionChoiceRepository.GetUserChoicesByUserIdAsync(currentUserId);

                // Group choices by question
                var questionGroups = userChoices.GroupBy(uc => uc.SurveyOption.SurveyQuestion);

                var responseDto = questionGroups.Select(group =>
                {
                    var question = group.Key;
                    var selectedOptions = group.Select(uc => new SurveyOptionResponseDTO
                    {
                        SurveyOptionId = uc.SurveyOption.SurveyOptionId,
                        SurveyOptionContent = uc.SurveyOption.SurveyOptionContent ?? string.Empty,
                        IsActive = uc.SurveyOption.IsAcive,
                        CreatedAt = uc.SurveyOption.CreatedAt,
                        SurveyQuestionId = uc.SurveyOption.SurveyQuestionId
                    }).ToList();

                    return new SurveyQuestionResponseDTO
                    {
                        SurveyQuestionId = question.SurveyQuestionId,
                        SurveyQuestionContent = question.SurveyQuestionName ?? string.Empty,
                        IsActive = question.IsActive,
                        CreatedAt = question.CreatedAt,
                        UpdatedAt = question.UpdatedAt,
                        CreatedBy = question.CreatedBy,
                        CreatedByName = question.CreatedByUser?.Fullname ?? "Unknown",
                        Options = selectedOptions
                    };
                }).ToList();

                return new ApiResponse<List<SurveyQuestionResponseDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = responseDto,
                    Message = "User survey history retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at GetUserSurveyHistoryAsync in SurveyService: {ex}", ex.Message);
                return new ApiResponse<List<SurveyQuestionResponseDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>> CheckUserSurveyStatusAsync()
        {
            try
            {
                var currentUserId = _currentUserService.AccountId;
                var user = await _userRepository.GetUserByIDAsync(currentUserId);

                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = user?.IsSurveyed ?? false,
                    Message = "User survey status retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at CheckUserSurveyStatusAsync in SurveyService: {ex}", ex.Message);
                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        private SurveyQuestionResponseDTO MapToSurveyQuestionResponseDTO(SurveyQuestion question)
        {
            return new SurveyQuestionResponseDTO
            {
                SurveyQuestionId = question.SurveyQuestionId,
                SurveyQuestionContent = question.SurveyQuestionName ?? string.Empty,
                IsActive = question.IsActive,
                CreatedAt = question.CreatedAt,
                UpdatedAt = question.UpdatedAt,
                CreatedBy = question.CreatedBy,
                CreatedByName = question.CreatedByUser?.Fullname ?? "Unknown",
                Options = question.SurveyOptions?.Select(option => new SurveyOptionResponseDTO
                {
                    SurveyOptionId = option.SurveyOptionId,
                    SurveyOptionContent = option.SurveyOptionContent ?? string.Empty,
                    IsActive = option.IsAcive,
                    CreatedAt = option.CreatedAt,
                    SurveyQuestionId = option.SurveyQuestionId
                }).ToList() ?? new List<SurveyOptionResponseDTO>()
            };
        }

        private SimpleSurveyQuestionResponseDTO MapToSimpleSurveyQuestionResponseDTO(SurveyQuestion question)
        {
            return new SimpleSurveyQuestionResponseDTO
            {
                SurveyQuestionId = question.SurveyQuestionId,
                SurveyQuestionContent = question.SurveyQuestionName ?? string.Empty,
                Options = question.SurveyOptions?.Select(option => new SimpleSurveyOptionResponseDTO
                {
                    SurveyOptionId = option.SurveyOptionId,
                    SurveyOptionContent = option.SurveyOptionContent ?? string.Empty
                }).ToList() ?? new List<SimpleSurveyOptionResponseDTO>()
            };
        }
    }
}
