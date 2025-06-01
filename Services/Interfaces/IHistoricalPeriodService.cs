using BusinessObjects.Models;
using Helpers.DTOs.HistoricalPeriod;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IHistoricalPeriodService
    {
        public Task<ApiResponse<PaginatedResult<HistoricalPeriodDTO>>> GetPagedHistoricalPeriodsAsync(int pageNumber, int pageSize);
        public Task<ApiResponse<HistoricalPeriod>> CreateHistoricalPeriod(HistoricalPeriodDTO historicalPeriodDto);
    }
}
