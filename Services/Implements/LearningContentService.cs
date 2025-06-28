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
        public LearningContentService(ILearningContentRepository learningContentRepository, 
            ILogger<LearningContentService> logger, 
            ICurrentUserService currentUserService,
            IFileHandlerService fileHandlerService
            )
        {
            _learningContentRepository = learningContentRepository;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileHandlerService = fileHandlerService;
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
    }
}
