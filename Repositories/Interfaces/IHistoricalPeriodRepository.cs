using BusinessObjects.Models;
using Helpers.DTOs.HistoricalPeriod;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IHistoricalPeriodRepository
    {
        public Task<HistoricalPeriod> CreateUserAsync(HistoricalPeriod historicalPeriod);

        public Task<PaginatedResult<HistoricalPeriodDTO>> GetPagedHistoricalPeriodsAsync(int pageNumber, int pageSize);
    }
}
