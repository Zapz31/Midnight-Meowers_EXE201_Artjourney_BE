using BusinessObjects.Models;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class LoginHistoryRepository : ILoginHistoryRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        public LoginHistoryRepository(IUnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<LoginHistory> CreateLoginHistory(LoginHistory loginHistory)
        {
            var createdLoginHistory = await _unitOfWork.GetRepo<LoginHistory>().CreateAsync(loginHistory);
            await _unitOfWork.SaveChangesAsync();
            return createdLoginHistory;
        }

        public async Task<long> GetMaxLoginHistoryIdAsync()
        {
            var queryOptions = new QueryBuilder<LoginHistory>()
                .WithPredicate(lh => lh.UserId != null)   
                .WithTracking(false)                      
                .Build();
            var maxId = await _unitOfWork.GetRepo<LoginHistory>()
                .MaxAsync(queryOptions, lh => (long?)lh.LoginHistoryId);

            return maxId ?? 0;
        }

        public async Task<long> CountLoginHistoriesByUserIdAsync(long userId)
        {
            var queryOptions = new QueryBuilder<LoginHistory>()
                .WithTracking(false)
                .WithPredicate(lh => lh.UserId == userId)
                .Build();

            var count = await _unitOfWork.GetRepo<LoginHistory>()
                .CountAsync(queryOptions);
            return count;
        } 
    }
}
