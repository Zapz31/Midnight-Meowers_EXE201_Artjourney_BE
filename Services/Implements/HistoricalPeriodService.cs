using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.HistoricalPeriod;
using Helpers.HelperClasses;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Services.Implements
{
    public class HistoricalPeriodService : IHistoricalPeriodService
    {
        private readonly IHistoricalPeriodRepository _historicalRepository;
        private readonly ILogger<HistoricalPeriodService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private static readonly Regex YearEraRegex = new Regex(@"^[1-9]\d*\s(BC|AD)$", RegexOptions.Compiled);
        public HistoricalPeriodService(IHistoricalPeriodRepository historicalRepository,
            ILogger<HistoricalPeriodService> logger,
            ICurrentUserService currentUserService
            ) 
        {
            _historicalRepository = historicalRepository;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<PaginatedResult<HistoricalPeriodDTO>>> GetPagedHistoricalPeriodsAsync(int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Start the GetPagedHistoricalPeriodsAsync at HistoricalPeriodService - line 28");
                var data = await _historicalRepository.GetPagedHistoricalPeriodsAsync(pageNumber, pageSize);
                ApiResponse<PaginatedResult<HistoricalPeriodDTO>> response = new()
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = data,
                    Message = "Get all HistoricalPeriods success"
                };
                return response;
            } catch (Exception ex)
            {
                _logger.LogError("Error at GetPagedHistoricalPeriodsAsync(int pageNumber, int pageSize) - line 31: {ex}", ex.Message);
                ApiResponse<PaginatedResult<HistoricalPeriodDTO>> response = new()
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "Get all HistoricalPeriods failed"
                };
                return response;
            }
        }

        public async Task<ApiResponse<HistoricalPeriod>> CreateHistoricalPeriod(HistoricalPeriodDTO historicalPeriodDto)
        {
            try
            {
                
                _logger.LogInformation("Start the CreateHistoricalPeriod at HistoricalPeriodService - line 61");
                ApiResponse<HistoricalPeriod> validateErrResponse = ValidateHistoricalPeriodField(historicalPeriodDto);
                if (validateErrResponse.Errors.Any())
                {
                    validateErrResponse.Status = ResponseStatus.Error;
                    validateErrResponse.Code = 400;
                    return validateErrResponse;
                }

                
                var createdBy = _currentUserService.AccountId;
                var createdByRole = _currentUserService.Role;
                if (string.IsNullOrEmpty(createdByRole) || !createdByRole.Equals(AccountRole.Admin.ToString())) 
                {
                    ApiResponse<HistoricalPeriod> permissionErrorResponse = new()
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,                
                        Message = "You don't have permission to do this action"
                    };
                    return permissionErrorResponse;
                }

                HistoricalPeriod historicalPeriod = new HistoricalPeriod();
                historicalPeriod.HistoricalPeriodName = historicalPeriodDto.HistoricalPeriodName;
                historicalPeriod.Description = historicalPeriodDto.Description;
                historicalPeriod.StartYear = historicalPeriodDto.StartYear;
                historicalPeriod.EndYear = historicalPeriodDto.EndYear;
                historicalPeriod.CreatedAt = DateTime.UtcNow;
                historicalPeriod.CreatedBy = createdBy;
                var data = await _historicalRepository.CreateUserAsync(historicalPeriod);

                ApiResponse<HistoricalPeriod> response = new()
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = data,
                    Message = "Create HistoricalPeriods success"
                };
                return response;
            } catch (Exception ex)
            {
                _logger.LogError("Error at CreateHistoricalPeriod(HistoricalPeriod historicalPeriod) - line 67: {ex}", ex.Message);
                ApiResponse<HistoricalPeriod> response = new()
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "Create HistoricalPeriods failed: " + ex.Message
                };
                return response;
            }
        }

        public ApiResponse<HistoricalPeriod> ValidateHistoricalPeriodField (HistoricalPeriodDTO historicalPeriod)
        {
            List<ApiError> errors = new List<ApiError>();
            if (string.IsNullOrWhiteSpace(historicalPeriod.HistoricalPeriodName))
            {
                ApiError error = new()
                {
                    Code = 400,
                    Field = "historical_period_name",
                    Message = "historical period name cannot null or empty"
                };
                errors.Add(error);
            }

            if (string.IsNullOrWhiteSpace(historicalPeriod.StartYear))
            {
                ApiError error = new()
                {
                    Code = 400,
                    Field = "start_year",
                    Message = " start_year cannot null or empty"
                };
                errors.Add(error);
            }
            else if (!YearEraRegex.IsMatch(historicalPeriod.StartYear))
            {
                ApiError error = new()
                {
                    Code = 400,
                    Field = "start_year",
                    Message = " start_year must follow this format: integer + space + AD or BC"
                };
                errors.Add(error);
            }

            if (string.IsNullOrWhiteSpace(historicalPeriod.EndYear))
            {
                ApiError error = new()
                {
                    Code = 400,
                    Field = "end_year",
                    Message = " end_year cannot null or empty"
                };
                errors.Add(error);
            }
            else if (!YearEraRegex.IsMatch(historicalPeriod.EndYear))
            {
                ApiError error = new()
                {
                    Code = 400,
                    Field = "end_year",
                    Message = "end_year must follow this format: integer + space + AD or BC"
                };
                errors.Add(error);
            }

            ApiResponse<HistoricalPeriod> validateResponse = new()
            {
                Errors = errors,
            };
            return validateResponse;

        }

    }
}
