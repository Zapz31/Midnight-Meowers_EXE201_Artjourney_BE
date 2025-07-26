using BusinessObjects.Enums;
using BusinessObjects.Models;
using DAOs;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class UserPremiumInfoRepository : IUserPremiumInfoRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public UserPremiumInfoRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<UserPremiumInfo> CreateUserPremiumInfo(UserPremiumInfo userPremiumInfo)
        {
            var createUserPremiumInfo = await _unitOfWork.GetRepo<UserPremiumInfo>().CreateAsync(userPremiumInfo);
            await _unitOfWork.SaveChangesAsync();
            return createUserPremiumInfo;
        }

        public async Task<UserPremiumInfo?> GetByUserIdAndStatus(long userId, UserPremiumStatus status)
        {
            var query = new QueryBuilder<UserPremiumInfo>()
                .WithTracking(false)
                .WithPredicate(ups => ups.UserId == userId && ups.Status == status)
                .Build();
            var responseData = await _unitOfWork.GetRepo<UserPremiumInfo>().GetSingleAsync(query);
            return responseData;
        }

        public async Task<List<UserPremiumInfo>> GetUserPremiumInfosByUserId(long userId)
        {
            var query = new QueryBuilder<UserPremiumInfo>()
                .WithTracking(false)
                .WithPredicate(ups => ups.UserId == userId)
                .Build();
            var responseData = await _unitOfWork.GetRepo<UserPremiumInfo>().GetAllAsync(query);
            return responseData.ToList();
        }

        public async Task<UserPremiumInfo?> GetLatestPremiumInfoByUserIdAsync(long inputUserId)
        {
            return await _context.UserPremiumInfos
                .Where(p => p.UserId == inputUserId)
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefaultAsync();
        }
    }
}
