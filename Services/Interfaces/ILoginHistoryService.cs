using BusinessObjects.Models;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ILoginHistoryService
    {
        Task<ApiResponse<LoginHistory?>> CreateLoginHistoryAsync(LoginHistory loginHistory);
        Task<ApiResponse<long>> GetMaxLoginHistoryIdAsync();

        Task<ApiResponse<long>> CountLoginHistoriesByUserIdAsync(long userId);
    }
}
