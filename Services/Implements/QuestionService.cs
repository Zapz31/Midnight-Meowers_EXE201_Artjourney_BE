using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.LearningContent;
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

        public QuestionService(IQuestionRepository questionRepository, 
            IQuestionOptionRepository optionRepository, 
            ILogger<QuestionService> logger,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService
            )
        {
            _questionRepository = questionRepository;
            _optionRepository = optionRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<bool>> CreateQuestionsAndOptionsAsync(List<CreateQuestionsAndOptionsBasicRequestDTO> CreateQuestionsAndOptionsBasicRequestDTOs)
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
                var responseData = await _questionRepository.CreateQuestionsWithOptionsBulkAsync(CreateQuestionsAndOptionsBasicRequestDTOs);
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
    }
}
