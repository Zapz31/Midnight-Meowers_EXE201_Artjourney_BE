using BusinessObjects.Models;
using Helpers.DTOs.HistoricalPeriod;
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
    public class HistoricalPeriodRepository : IHistoricalPeriodRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        public HistoricalPeriodRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<HistoricalPeriod> CreateUserAsync(HistoricalPeriod historicalPeriod, List<long> regionIds)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var createdHisroticalPeriod = await _unitOfWork.GetRepo<HistoricalPeriod>().CreateAsync(historicalPeriod);
                await _unitOfWork.SaveChangesAsync();

                List<RegionHisoricalPeriod> requestRegionHistoricalPeriods = new List<RegionHisoricalPeriod>();
                foreach (long regionId in regionIds)
                {
                    RegionHisoricalPeriod regionHistoricalPeriod = new RegionHisoricalPeriod()
                    {
                        RegionId = regionId,
                        HistoricalPeriodId = createdHisroticalPeriod.HistoricalPeriodId
                    };
                    requestRegionHistoricalPeriods.Add(regionHistoricalPeriod);
                }

                if (requestRegionHistoricalPeriods.Count > 0)
                {
                    var regionHistoricalPeriodRepo = _unitOfWork.GetRepo<RegionHisoricalPeriod>();
                    await regionHistoricalPeriodRepo.CreateAllAsync(requestRegionHistoricalPeriods);
                    await _unitOfWork.SaveChangesAsync();
                }
                await _unitOfWork.CommitTransactionAsync();

                return createdHisroticalPeriod;
            } catch (Exception ex)
            {
                await _unitOfWork.RollBackAsync();
                throw new Exception($"Error creating historical period with regions in historicalPeriodRepository: {ex.Message}", ex);
            }
            
        }

        /// <summary>
        /// Get all HistoricalPeriods with (DeletedAt == null), 
        /// Ascending sort by HistoricalPeriodName and paging (5 records/ page).
        /// </summary>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size (default = 5)</param>
        /// <returns>PaginatedResult chứa Items (IEnumerable&lt;HistoricalPeriod&gt;), TotalCount, PageNumber, PageSize, TotalPages.</returns>
        public async Task<PaginatedResult<HistoricalPeriodDTO>> GetPagedHistoricalPeriodsAsync(int pageNumber, int pageSize)
        {
            // Xây dựng QueryOptions: lọc DeletedAt == null, sắp xếp theo HistoricalPeriodName
            var options = new QueryBuilder<HistoricalPeriod>()
                .WithTracking(false)
                .WithPredicate(hp => hp.DeletedAt == null)
                .WithOrderBy(q => q.OrderBy(hp => hp.HistoricalPeriodName))
                .Build();

            var query = _unitOfWork.GetRepo<HistoricalPeriod>().Get(options);

            var pagedResult = await Pagination.ApplyPaginationAsync(
              query,
              pageNumber,
              pageSize,
              h => new HistoricalPeriodDTO
              {
                  HistoricalPeriodId = h.HistoricalPeriodId,
                  HistoricalPeriodName = h.HistoricalPeriodName,
                  Description = h.Description,
                  StartYear = h.StartYear,
                  EndYear = h.EndYear,
              }
            );

            return pagedResult;
        }

        public async Task<List<HistoricalPeriodDTO>> GetAllHistoricalPeriodsDropdownByRegionIdAsync(long regionId)
        {
            var options = new QueryBuilder<HistoricalPeriod>()
                .WithTracking(false)
                .WithPredicate(hp => hp.DeletedAt == null)
                .WithOrderBy(q => q.OrderBy(hp => hp.HistoricalPeriodName))
                .Build();

            var historicalPeriods = await _unitOfWork.GetRepo<HistoricalPeriod>().GetAllAsync(options);

            var result = historicalPeriods.Select(hp => new HistoricalPeriodDTO
            {
                HistoricalPeriodId = hp.HistoricalPeriodId,
                HistoricalPeriodName = hp.HistoricalPeriodName,
                Description = hp.Description,
                StartYear = hp.StartYear,
                EndYear = hp.EndYear,
            }).ToList();
            return result;
        }
    }
}
