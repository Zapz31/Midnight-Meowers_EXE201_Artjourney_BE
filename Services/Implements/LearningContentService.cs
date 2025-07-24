using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.ChallengeItem;
using Helpers.DTOs.LearningContent;
using Helpers.HelperClasses;
using Microsoft.AspNetCore.Http;
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
    public class LearningContentService : ILearningContentService
    {
        private readonly ILearningContentRepository _learningContentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<LearningContentService> _logger;
        private readonly IFileHandlerService _fileHandlerService;
        private readonly string ImageBaseUrl = "https://zapzminio.phrimp.io.vn/";
        private readonly IQuizAttemptRepository _quizAttemptRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserAnswerRepository _userAnswerRepository;
        private readonly IUserService _userService;
        private readonly IUserLearningProgressRepository _userLearningProgressRepository;
        public LearningContentService(ILearningContentRepository learningContentRepository, 
            ILogger<LearningContentService> logger, 
            ICurrentUserService currentUserService,
            IFileHandlerService fileHandlerService,
            IQuizAttemptRepository quizAttemptRepository,
            IUnitOfWork unitOfWork,
            IUserAnswerRepository userAnswerRepository,
            IUserService userService,
            IUserLearningProgressRepository userLearningProgressRepository
            )
        {
            _learningContentRepository = learningContentRepository;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileHandlerService = fileHandlerService;
            _quizAttemptRepository = quizAttemptRepository;
            _unitOfWork = unitOfWork;
            _userAnswerRepository = userAnswerRepository;
            _userService = userService;
            _userLearningProgressRepository = userLearningProgressRepository;
        }

        public async Task<ApiResponse<LearningContent>> CreateLNContentReadingAsync(CreateLNReadingDTO requestDTO)
        {
            try
            {
                var createUserId = _currentUserService.AccountId;
                var role = _currentUserService.Role;
                var status = _currentUserService.Status;
                if (role == null || !(AccountRole.Admin.ToString().Equals(role)) || !(AccountStatus.Active.ToString().Equals(status)))
                {
                    return new ApiResponse<LearningContent>
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Message = "You don't have permission to do this action"
                    };
                }

                LearningContent createLearningContentId = new()
                {
                    Title = requestDTO.Title,
                    TimeLimit = requestDTO.TimeLimit,
                    DisplayOrder = requestDTO.DisplayOrder,
                    SubModuleId = requestDTO.SubModuleId,
                    ContentType = LearningContentType.Reading,
                    CreatedBy = createUserId,
                    CourseId = requestDTO.CourseId,
                };
                List<ChallengeItem> challengeItems = new List<ChallengeItem>();
                var createdLearningContent = await _learningContentRepository.CreateLearningContentAsync(createLearningContentId);

                if (requestDTO.Video != null)
                {
                    ChallengeItem challengeItem = new ChallengeItem();
                    List<IFormFile> videos = new List<IFormFile>();
                    string? videoUrl = null;
                    videos.Add(requestDTO.Video);
                    string? realItemContent = null;
                    var uploadFilesResult = await _fileHandlerService.UploadFiles(videos, "a", "a");
                    if (uploadFilesResult.Errors.Any())
                    {
                        ApiResponse<Course> UploadFileErrorResponse = new ApiResponse<Course>()
                        {
                            Status = ResponseStatus.Error,
                            Code = 500,
                            Message = "Server error"
                        };
                    }
                    videoUrl = uploadFilesResult.SuccessfulUploads[0].PresignedUrl;
                    Uri uri = new Uri(videoUrl);
                    string pathAndFileName = uri.PathAndQuery.TrimStart('/');
                    realItemContent = ImageBaseUrl + pathAndFileName;
                    challengeItem.ItemContent = realItemContent;
                    challengeItem.ItemTypes = ChallengeItemTypes.Video;
                    //challengeItem.ItemOrder = 1;
                    challengeItem.LearningContentId = createLearningContentId.LearningContentId;
                    challengeItems.Add(challengeItem);
                }

                if (!string.IsNullOrWhiteSpace(requestDTO.Content))
                {
                    ChallengeItem challengeItem = new ChallengeItem();
                    challengeItem.ItemContent = requestDTO.Content;
                    challengeItem.ItemTypes = ChallengeItemTypes.Text;
                    challengeItem.LearningContentId = createLearningContentId.LearningContentId;
                    challengeItems.Add(challengeItem);
                }

                await _learningContentRepository.CreateAllChallengeItemAsync(challengeItems);
                return new ApiResponse<LearningContent>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = createdLearningContent,
                    Message = "Create learning content success"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at CreateLNContentReadingAsync at LearningContentService.cs: {ex}", ex.Message);
                return new ApiResponse<LearningContent>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
            
        }

        public async Task<ApiResponse<List<BasicLearningContentGetResponseDTO>>> GetLearningContentsBySubmoduleId(long subModuleId)
        {
            try
            {
                var data = await _learningContentRepository.GetLearningContentsBySubModuleIdAsync(subModuleId);
                return new ApiResponse<List<BasicLearningContentGetResponseDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = data,
                    Message = "Data retrive success"
                };
            } catch (Exception ex)
            {
                _logger.LogError("Error at GetLearningContentsBySubmoduleId at LearningContentService.cs: {ex}", ex.Message);
                return new ApiResponse<List<BasicLearningContentGetResponseDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<BasicChallengeItemGetResponseDTO>>> GetChallengeItemsByLNCId(long learningContentId)
        {
            try
            {
                var data = await _learningContentRepository.GetChallengeItemsByLNCId(learningContentId);
                return new ApiResponse<List<BasicChallengeItemGetResponseDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = data,
                    Message = "Data retrive success"
                };
            } catch (Exception ex)
            {
                _logger.LogError("Error at GetChallengeItemsByLNCId at LearningContentService.cs: {ex}", ex.Message);
                return new ApiResponse<List<BasicChallengeItemGetResponseDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<QuizAttempt>> StartQuizAsync(long userId, long learningContentId)
        {
            try
            {
                var learningConent = await _learningContentRepository.GetLearningContentById(learningContentId);
                if (learningConent == null || learningConent.CompleteCriteria == null)
                {
                    return new ApiResponse<QuizAttempt>
                    {
                        Status = ResponseStatus.Error,
                        Code = 500,
                        Message = "Error at StartQuizAsync at LearningContentService.cs: learningConent or complete_criteria is null"
                    };
                }

                QuizAttempt quizAttempt = new()
                {
                    StartedAt = DateTime.UtcNow,
                    LearningContentId = learningContentId,
                    UserId = userId,
                    TotalPossibleScore = learningConent.CompleteCriteria ?? 0
                };
                var responseData = await _quizAttemptRepository.CreateQuizAttempt(quizAttempt);
                return new ApiResponse<QuizAttempt>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Message = "Create Quiz attempt successfully",
                    Data = responseData
                };
            } catch (Exception ex)
            {
                _logger.LogError("Error at StartQuizAsync at LearningContentService.cs: {ex}", ex.Message);
                return new ApiResponse<QuizAttempt>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<LearningContent>> CreateQuizTitle(CreateQuizTitleRequestDTO createQuizTitleRequestDTO)
        {
            try
            {
                var userId = _currentUserService.AccountId;
                var role = _currentUserService.Role;
                var status = _currentUserService.Status;
                if (role == null || !(AccountRole.Admin.ToString().Equals(role)) || !(AccountStatus.Active.ToString().Equals(status)))
                {
                    return new ApiResponse<LearningContent>
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Message = "You don't have permission to do this action"
                    };
                }
                var createLearningContent = new LearningContent()
                {
                    ContentType = createQuizTitleRequestDTO.ContentType,
                    Title = createQuizTitleRequestDTO.Title,
                    TimeLimit = createQuizTitleRequestDTO.TimeLimit,
                    DisplayOrder = createQuizTitleRequestDTO.DisplayOrder,
                    CreatedBy = userId,
                    SubModuleId = createQuizTitleRequestDTO.SubModuleId,
                    CourseId = createQuizTitleRequestDTO.CourseId,

                };
                var responseData = await _learningContentRepository.CreateLearningContentAsync(createLearningContent);
                return new ApiResponse<LearningContent>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = responseData,
                    Message = "Quiz create success"
                };
            } catch (Exception ex)
            {
                _logger.LogError("Error at CreateQuizTitle at LearningContentService.cs: {ex}", ex.Message);
                return new ApiResponse<LearningContent>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>> SubmitQuizAsync(SubmitQuizRequestDTO submitQuizRequestDTO)
        {
            try
            {
                
                
                var userId = _currentUserService.AccountId;
                List<UserAnswer> createdUserAnswers = new();
                foreach(var requestUserAnswer in submitQuizRequestDTO.UserAnswers)
                {
                    var userAnswer = new UserAnswer
                    {
                        QuizAttemptId = requestUserAnswer.QuizAttemptId,
                        QuestionId = requestUserAnswer.QuestionId,
                        SelectedOptionId = requestUserAnswer.SelectedOptionId,
                    };
                    createdUserAnswers.Add(userAnswer);
                }

                // insert user_answers
                await _userAnswerRepository.CreateUserAnswers(createdUserAnswers);
                // calculate total score
                var totalScore = await _userAnswerRepository.CalculateTotalScoreAsync(submitQuizRequestDTO.QuizAttemptId);
                //update quiz_attempts
                var updatedRow = await _quizAttemptRepository.UpdateQuizAttemptWithSubmitQuiz(submitQuizRequestDTO.QuizAttemptId, totalScore);
                if(updatedRow < 1)
                {
                    return new ApiResponse<bool>
                    {
                        Status = ResponseStatus.Error,
                        Code = 500,
                        Message = "Error when update quiz_attempts: No row has updated"
                    };
                }
                //mark as complete
                
                await _userService.MarkAsCompleteUserLearningProgressSingleAsync(submitQuizRequestDTO.LearningContentId);
                
                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = true,
                    Message = "Submit quiz successful"
                };
            } catch (Exception ex)
            {
                
                _logger.LogError("Error at SubmitQuizAsync at LearningContentService.cs: {ex}", ex.Message);
                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<int>> SoftDeleteLearningContentAsync(long learningContentIdInput)
        {
            try
            {
                var responseData = await _learningContentRepository.UpdateLearningContentIsActiveAsync(learningContentIdInput);
                return new ApiResponse<int>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = responseData,
                    Message = "Remove success"
                };
            }catch (Exception ex) 
            {
                _logger.LogError("Error at SoftDeleteLearningContentAsync at LearningContentService.cs: {ex}", ex.Message);
                return new ApiResponse<int>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message,
                    Data = 0
                };
            }
        }
    }
}
