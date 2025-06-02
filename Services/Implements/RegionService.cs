using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.HistoricalPeriod;
using Helpers.DTOs.Regions;
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
    public class RegionService : IRegionService
    {
        private readonly IRegionRepository _regionRepository;
        private readonly ILogger<RegionService> _logger;
        private readonly ICurrentUserService _currentUserService;

        public RegionService(IRegionRepository regionRepository, 
            ILogger<RegionService> logger, 
            ICurrentUserService currentUserService)
        {
            _regionRepository = regionRepository;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<PaginatedResult<RegionDTO>>> GetPagedRegionsAsync(int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Start the  at GetPagedRegionsAsync - line 36");
                var data = await _regionRepository.GetPagedRegionsAsync(pageNumber, pageSize);
                ApiResponse<PaginatedResult<RegionDTO>> response = new()
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = data,
                    Message = "Get all Regions success"
                };
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at GetPagedHistoricalPeriodsAsync(int pageNumber, int pageSize): {ex}", ex.Message);
                ApiResponse<PaginatedResult<RegionDTO>> response = new()
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "Get all Regions  failed"
                };
                return response;
            }
        }

        public async Task<ApiResponse<Region>> CreateRegionAsync(RegionDTO regionDTO)
        {
            try
            {
                ApiResponse<Region> validateResponse = ValidateRegionRequest(regionDTO);
                if (validateResponse.Errors.Any())
                {
                    validateResponse.Status = ResponseStatus.Error;
                    validateResponse.Code = 401;
                    return validateResponse;
                }

                var createdBy = _currentUserService.AccountId;
                var currentUserRole = _currentUserService.Role;
                if (string.IsNullOrEmpty(currentUserRole) || !AccountRole.Admin.ToString().Equals(currentUserRole))
                {
                    ApiResponse<Region> permissionErrorResponse = new()
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Message = "You don't have permission to do this action"
                    };
                    return permissionErrorResponse;
                }

                Region region = new Region()
                {
                    RegionName = regionDTO.RegionName,
                    Description = regionDTO.Description,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow,
                };
                var data = await _regionRepository.CreateRegionAsync(region);
                ApiResponse<Region> response = new()
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = data,
                    Message = "Create Regions success"
                };
                return response;
            }
            catch(Exception ex)
            {
                _logger.LogError("Error at CreateRegion - line 75: {ex}", ex.Message);
                ApiResponse<Region> response = new()
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "Create HistoricalPeriods failed: " + ex.Message
                };
                return response;
            }
        }



        public ApiResponse<Region> ValidateRegionRequest(RegionDTO regionDTO) 
        {
            List<ApiError> errors = new List<ApiError>();
            if (string.IsNullOrWhiteSpace(regionDTO.RegionName))
            {
                ApiError apiError = new()
                {
                    Field = "region_name",
                    Message = "region_name cannot be empty"
                };
                errors.Add(apiError);
            }



            ApiResponse<Region> validateResponse = new()
            {
                Errors = errors,
            };
            return validateResponse;


        }




    }
}
