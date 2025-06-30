using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.LearningContent;
using Helpers.DTOs.Question;
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
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IQuestionOptionRepository _optionRepository;
        private readonly ILogger<QuestionService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILearningContentRepository _learningContentRepository;

        public QuestionService(IQuestionRepository questionRepository, 
            IQuestionOptionRepository optionRepository, 
            ILogger<QuestionService> logger,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILearningContentRepository learningContentRepository
            )
        {
            _questionRepository = questionRepository;
            _optionRepository = optionRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _learningContentRepository = learningContentRepository;
        }

        public async Task<ApiResponse<bool>> CreateQuestionsAndOptionsAsync(List<CreateQuestionsAndOptionsBasicRequestDTO> createQuestionsAndOptionsBasicRequestDTOs)
        {
            try
            {
                var createUserId = _currentUserService.AccountId;
                var role = _currentUserService.Role;
                var status = _currentUserService.Status;
                if (role == null || !(AccountRole.Admin.ToString().Equals(role)) || !(AccountStatus.Active.ToString().Equals(status)))
                {
                    return new ApiResponse<bool>
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Message = "You don't have permission to do this action"
                    };
                }
                if (createQuestionsAndOptionsBasicRequestDTOs.Count < 1 || createQuestionsAndOptionsBasicRequestDTOs == null)
                {
                    return new ApiResponse<bool>
                    {
                        Status = ResponseStatus.Error,
                        Code = 400,
                        Message = "There must be at least 1 question"
                    };
                }

                var responseData = await _questionRepository.CreateQuestionsWithOptionsBulkAsync(createQuestionsAndOptionsBasicRequestDTOs);
                var learningContentId = createQuestionsAndOptionsBasicRequestDTOs[0].LearningContentId;

                // total point of this quiz
                var totalPoint = createQuestionsAndOptionsBasicRequestDTOs.Sum(q => q.Points);

                var updateLearningContentRow = await _learningContentRepository.UpdateCompleteCriteria(learningContentId, totalPoint);
                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Message = "Data was created successfully",
                    Data = responseData
                };
            } catch (Exception ex)
            {
                _logger.LogError("Error at CreateQuestionsAndOptionsAsync in QuestionService: {ex}", ex.Message);
                ApiResponse<bool> errorResponse = new ApiResponse<bool>()
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "Server error"
                };
                return errorResponse;
            }
        }

        public async Task<ApiResponse<PaginatedResult<GetQuestionQuizDTO>>> GetQuestionWithOptionQuizAsync(long learningContentId, int pageNumber, int pageSize)
        {
            try
            {
                var responesData = await _questionRepository.GetQuestionWithOptionQuizAsync(learningContentId, pageNumber, pageSize);
                return new ApiResponse<PaginatedResult<GetQuestionQuizDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = responesData,
                    Message = "Data retrive successfully"
                };
            } catch (Exception ex)
            {
                _logger.LogError("Error at CreateQuestionsAndOptionsAsync in QuestionService: {ex}", ex.Message);
                ApiResponse<PaginatedResult<GetQuestionQuizDTO>> errorResponse = new ApiResponse<PaginatedResult<GetQuestionQuizDTO>>()
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "Server error"
                };
                return errorResponse;
            }
        }

        
    }
}
