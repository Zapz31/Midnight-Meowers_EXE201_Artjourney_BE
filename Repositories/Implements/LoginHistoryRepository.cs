using BusinessObjects.Models;
using Repositories.Interfaces;
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
            var maxId = await _unitOfWork.GetRepo<LoginHistory>()
                .MaxAsync(lh => (long?)lh.LoginHistoryId);

            return maxId ?? 0;
        }
    }
}
