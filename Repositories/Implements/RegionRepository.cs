using BusinessObjects.Models;
using Helpers.DTOs.HistoricalPeriod;
using Helpers.DTOs.Regions;
using Helpers.HelperClasses;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class RegionRepository : IRegionRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        public RegionRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Region> CreateRegionAsync(Region region)
        {
            var createdRegion = await _unitOfWork.GetRepo<Region>().CreateAsync(region);
            await _unitOfWork.SaveChangesAsync();
            return createdRegion;
        }

        public async Task<PaginatedResult<RegionDTO>> GetPagedRegionsAsync(int pageNumber, int pageSize)
        {
            // Xây dựng QueryOptions: lọc DeletedAt == null, sắp xếp theo HistoricalPeriodName
            var options = new QueryBuilder<Region>()
                .WithTracking(false)
                .WithPredicate(r => r.DeletedAt == null)
                .WithOrderBy(q => q.OrderBy(r => r.RegionName))
                .Build();

            var query = _unitOfWork.GetRepo<Region>().Get(options);

            var pagedResult = await Pagination.ApplyPaginationAsync(
              query,
              pageNumber,
              pageSize,
              r => new RegionDTO
              {
                  RegionName = r.RegionName,
                  RegionId = r.RegionId,
                  Description = r.Description,
              }
            );

            return pagedResult;
        }
    }
}
