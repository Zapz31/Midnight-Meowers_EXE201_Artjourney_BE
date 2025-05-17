using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ILoginHistoryRepository
    {
        Task<LoginHistory> CreateLoginHistory(LoginHistory loginHistory);
        public Task<long> GetMaxLoginHistoryIdAsync();
        public Task<long> CountLoginHistoriesByUserIdAsync(long userId);
    }
}
