using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.SubModule;
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
    public class SubModuleService : ISubModuleService
    {
        private readonly ISubModuleRepository _subModuleRepository;
        private readonly ILogger<SubModuleService> _logger;
        private readonly ICurrentUserService _currentUserService;

        public SubModuleService(ISubModuleRepository subModuleRepository, 
            ICurrentUserService currentUserService, 
            ILogger<SubModuleService> logger
            )
        {
            _subModuleRepository = subModuleRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<SubModule>> CreateSubModuleAsync(CreateSubModuleRequestDTO requestDTO)
        {
            try
            {
                var createUserId = _currentUserService.AccountId;
                var role = _currentUserService.Role;
                if (role == null || !(AccountRole.Admin.ToString().Equals(role)))
                {
                    return new ApiResponse<SubModule>
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Message = "You don't have permission to do this action"
                    };
                }

                SubModule subModule = new()
                {
                    SubModuleTitle = requestDTO.SubModuleTitle,
                    Description = requestDTO.Description,
                    DisplayOrder = requestDTO.DisplayOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createUserId,
                    ModuleId = requestDTO.ModuleId,
                };

                var createdData = await _subModuleRepository.CreateUserAsync(subModule);
                return new ApiResponse<SubModule>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = createdData,
                    Message = "Create submodule success"
                };

            } catch (Exception ex)
            {
                _logger.LogError("Error at CreateModuleAsync at ModuleService.cs: {ex}", ex.Message);
                return new ApiResponse<SubModule>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<BasicSubModuleGetResponseDTO>>> GetSubmodulesByModuleId(long moduleId)
        {
            try
            {
                var data = await _subModuleRepository.GetSubModulesByModuleIdAsync(moduleId);
                return new ApiResponse<List<BasicSubModuleGetResponseDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = data,
                    Message = "Sub-modules retrive success"
                };
            } catch (Exception ex)
            {
                _logger.LogError("Error at GetSubmodulesByModuleId at ModuleService.cs: {ex}", ex.Message);
                return new ApiResponse<List<BasicSubModuleGetResponseDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<int>> SoftDeleteSubModuleByIdAsync(long subModuleId)
        {
            try
            {
                var responseData = await _subModuleRepository.SoftDeleteSubModuleByIdAsync(subModuleId);
                return new ApiResponse<int>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = responseData,
                };
            } catch (Exception ex)
            {
                return new ApiResponse<int>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }
    }
}
