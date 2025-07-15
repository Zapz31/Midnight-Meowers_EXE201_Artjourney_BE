using BusinessObjects.Enums;
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IUserPremiumInfoRepository
    {
        public Task<UserPremiumInfo> CreateUserPremiumInfo(UserPremiumInfo userPremiumInfo);
        public Task<UserPremiumInfo?> GetByUserIdAndStatus(long userId, UserPremiumStatus status);
    }
}
