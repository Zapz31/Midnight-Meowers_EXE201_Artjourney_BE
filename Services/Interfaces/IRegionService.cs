using BusinessObjects.Models;
using Helpers.DTOs.Regions;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IRegionService
    {
        public Task<ApiResponse<PaginatedResult<RegionDTO>>> GetPagedRegionsAsync(int pageNumber, int pageSize);
        public Task<ApiResponse<Region>> CreateRegionAsync(RegionDTO regionDTO);
    }
}
