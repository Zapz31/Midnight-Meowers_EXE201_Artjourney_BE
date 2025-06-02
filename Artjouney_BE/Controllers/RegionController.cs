using BusinessObjects.Models;
using Helpers.DTOs.HistoricalPeriod;
using Helpers.DTOs.Regions;
using Helpers.HelperClasses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Implements;
using Services.Interfaces;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionController : ControllerBase
    {
        private readonly IRegionService _regionService;

        public RegionController(IRegionService regionService)
        {
            _regionService = regionService;
        }

        [HttpGet("/regions")]
        public async Task<IActionResult> GetPagedRegionsAsync([FromQuery] int page, [FromQuery] int size)
        {
            ApiResponse<PaginatedResult<RegionDTO>> response = await _regionService.GetPagedRegionsAsync(page, size);
            return StatusCode(response.Code, response);
        }

        [HttpPost("/regions")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateRegion([FromBody] RegionDTO regionDto)
        {
            ApiResponse<Region> response = await _regionService.CreateRegionAsync(regionDto);
            return StatusCode(response.Code, response);
        }
    }
}
