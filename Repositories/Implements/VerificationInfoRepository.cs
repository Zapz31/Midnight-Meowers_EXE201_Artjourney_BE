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
    public class VerificationInfoRepository : IVerificationInfoRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        public VerificationInfoRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> DeleteVerificationInfosByEmail(string email)
        {
            var queryOption = new QueryBuilder<VerificationInfo>()
                .WithPredicate(v => v.Email == email)
                .Build();

            var deletedRecord = await _unitOfWork.GetRepo<VerificationInfo>().ExecuteDeleteAsync(queryOption);
            return deletedRecord;
        }

        public async Task<VerificationInfo> CreateVerificationInfo(VerificationInfo verificationInfo)
        {
            var createdVerificationInfo = await _unitOfWork.GetRepo<VerificationInfo>().CreateAsync(verificationInfo);
            await _unitOfWork.SaveChangesAsync();
            return createdVerificationInfo;
        }

        public async Task<VerificationInfo?> GetVerificationInfoByEmail(string token)
        {
            var queryOptions = new QueryBuilder<VerificationInfo>()
                .WithTracking(false)
                .WithPredicate(v => v.Token.Equals(token))
                .Build();
            
            var verification = await _unitOfWork.GetRepo<VerificationInfo>().GetSingleAsync(queryOptions);
            return verification;
        }
    }
}
