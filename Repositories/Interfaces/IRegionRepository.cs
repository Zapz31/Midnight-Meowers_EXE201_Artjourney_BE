using BusinessObjects.Models;
using Helpers.DTOs.Regions;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IRegionRepository
    {
        public Task<Region> CreateRegionAsync(Region region);

        public Task<PaginatedResult<RegionDTO>> GetPagedRegionsAsync(int pageNumber, int pageSize);
    }
}
