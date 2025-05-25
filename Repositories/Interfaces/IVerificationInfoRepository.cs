using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IVerificationInfoRepository
    {
        public Task<int> DeleteVerificationInfosByEmail(string email);
        public Task<VerificationInfo> CreateVerificationInfo(VerificationInfo verificationInfo);
        public Task<VerificationInfo?> GetVerificationInfoByEmail(string token);
    }
}
