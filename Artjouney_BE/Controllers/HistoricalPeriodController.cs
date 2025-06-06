﻿using BusinessObjects.Models;
using Helpers.DTOs.HistoricalPeriod;
using Helpers.HelperClasses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoricalPeriodController : ControllerBase
    {
        private readonly IHistoricalPeriodService _historicalPeriodService;
        public HistoricalPeriodController(IHistoricalPeriodService historicalPeriodService)
        {
            _historicalPeriodService = historicalPeriodService;
        }

        [HttpGet("/api/historical-periods")]
        public async Task<IActionResult> GetPagedHistoricalPeriodsAsync([FromQuery] int page, [FromQuery] int size)
        {
            ApiResponse<PaginatedResult<HistoricalPeriodDTO>> response = await _historicalPeriodService.GetPagedHistoricalPeriodsAsync(page, size);
            return StatusCode(response.Code, response);
        }

        [HttpPost("/api/historical-periods")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateHistoricalPeriod ([FromBody] HistoricalPeriodDTO historicalPeriodDto)
        {
            ApiResponse<HistoricalPeriod> response = await _historicalPeriodService.CreateHistoricalPeriod(historicalPeriodDto);
            return StatusCode(response.Code, response);
        }

        [HttpGet("/api/historical-periods/region/{regionId}")]
        public async Task<IActionResult> GetPagedHistoricalPeriodsAsync(int regionId)
        {
            ApiResponse<List<HistoricalPeriodDTO>> response = await _historicalPeriodService.GetAllHistoricalPeriodsDropdownByRegionIdAsync(regionId);
            return StatusCode(response.Code, response);
        }

    }
}
