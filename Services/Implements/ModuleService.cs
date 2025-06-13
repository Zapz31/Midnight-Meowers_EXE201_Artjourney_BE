using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.Module;
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
    public class ModuleService : IModuleService
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly ILogger<ModuleService> _logger;
        private readonly ILearningContentRepository _learningContentRepository;
        private readonly ICurrentUserService _currentUserService;

        public ModuleService(IModuleRepository moduleRepository, 
            ILogger<ModuleService> logger, 
            ILearningContentRepository learningContentRepository,
            ICurrentUserService currentUserService
            )
        {
            _moduleRepository = moduleRepository;
            _logger = logger;
            _learningContentRepository = learningContentRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<Module>> CreateModuleAsync(CreateModuleRequestDTO requestDTO)
        {
            try
            {
                _logger.LogInformation("Start CreateModuleAsync at ModuleService.cs");
                var creatUserId = _currentUserService.AccountId;
                var role = _currentUserService.Role;
                if (role == null || !(AccountRole.Admin.ToString().Equals(role)))
                {
                    return new ApiResponse<Module>
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Message = "You don't have permission to do this action"
                    };
                }
                Module module = new()
                {
                    ModuleTitle = requestDTO.ModuleTitle,
                    Description = requestDTO.Description,
                    CreatedBy = creatUserId,
                    CourseId = requestDTO.CourseId,
                };
                var createdData = await _moduleRepository.CreateModuleAsync(module);
                return new ApiResponse<Module>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = createdData,
                    Message = "Create module success"
                };
            } catch (Exception ex)
            {
                _logger.LogError("Error at CreateModuleAsync at ModuleService.cs: {ex}", ex.Message);
                return new ApiResponse<Module>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<BasicModuleGetResponseDTO>>> GetModulesByCourseIdCompleteAsync(long courseId)
        {
            try
            {
                var data = await _moduleRepository.GetModulesByCourseIdCompletedAsync(courseId);
                return new ApiResponse<List<BasicModuleGetResponseDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = data,
                    Message = "Data retrive success"
                };
            } catch (Exception ex)
            {
                _logger.LogError("Error at GetModulesByCourseIdCompleteAsync at ModuleService.cs: {ex}", ex.Message);
                return new ApiResponse<List<BasicModuleGetResponseDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }
    }
}
