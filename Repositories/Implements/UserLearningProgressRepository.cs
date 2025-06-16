using BusinessObjects.Enums;
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
    public class UserLearningProgressRepository : IUserLearningProgressRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserLearningProgressRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<UserLearningProgress> CreateUserLearningProgressAsync(UserLearningProgress userLearningProgress)
        {
            var createdUserLearningProgress = await _unitOfWork.GetRepo<UserLearningProgress>().CreateAsync(userLearningProgress);
            await _unitOfWork.SaveChangesAsync();
            return createdUserLearningProgress;
        }

        public async Task CreateAllUserLearningProgressAsync(List<UserLearningProgress> userLearningProgresses)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                await _unitOfWork.GetRepo<UserLearningProgress>().CreateAllAsync(userLearningProgresses);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            } catch (Exception ex)
            {
                await _unitOfWork.RollBackAsync();
                throw ex;
            }
            
        }

        public async Task CreateUserLearningProgressesAsync(List<UserLearningProgress> userLearningProgresses)
        {
            await _unitOfWork.GetRepo<UserLearningProgress>().CreateAllAsync(userLearningProgresses);
        }

        public async Task<UserLearningProgress?> GetUserLearningProgressByUserIdAndLNCId(long userId, long learningContentId)
        {
            var options = new QueryBuilder<UserLearningProgress>()
                .WithTracking(false)
                .WithPredicate(ulp => ulp.UserId == userId &&  ulp.LearningContentId == learningContentId)
                .Build();

            var data = await _unitOfWork.GetRepo<UserLearningProgress>().GetSingleAsync(options);
            return data;
        }

        public async Task<List<long>> GetExistLCIdsByUserId(long userId, List<long> learningContentIdsByCourseIds, UserLearningProgressStatus? status)
        {
            QueryOptions<UserLearningProgress> mainOption = new QueryOptions<UserLearningProgress>();
            if (status == null)
            {
                var option = new QueryBuilder<UserLearningProgress>()
                .WithTracking(false)
                .WithPredicate(ulp => ulp.UserId == userId && learningContentIdsByCourseIds.Contains(ulp.LearningContentId))
                .Build();
                mainOption = option;
            } else
            {
                var option = new QueryBuilder<UserLearningProgress>()
                .WithTracking(false)
                .WithPredicate(ulp => ulp.UserId == userId && learningContentIdsByCourseIds.Contains(ulp.LearningContentId) && ulp.Status == status)
                .Build();
                mainOption = option;
            }
            var queryData = await _unitOfWork.GetRepo<UserLearningProgress>().GetAllAsync(mainOption);
            var existLearningContentIdByUserId = queryData.Select(ulp => ulp.LearningContentId).ToList();
            return existLearningContentIdByUserId;
        }

        public async Task<IEnumerable<UserLearningProgress>> GetLearningProgressByUserIdAndLNCIds(long userId, List<long> learningContentIds)
        {
            var progressQuery = new QueryBuilder<UserLearningProgress>()
                .WithPredicate(ulp => learningContentIds.Contains(ulp.LearningContentId) && ulp.UserId == userId)
                .Build();
            var userProgresses = await _unitOfWork.GetRepo<UserLearningProgress>().GetAllAsync(progressQuery);
            return userProgresses;
        }

        public async Task UpdateUserLearningProgressSingleAsync(UserLearningProgress userLearningProgress)
        {
            await _unitOfWork.GetRepo<UserLearningProgress>().UpdateAsync(userLearningProgress);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<UserLearningProgress?> GetLearningProgressByUserIdAndLNCIdSingle(long userId, long learningContentId)
        {
            var queryOtion = new QueryBuilder<UserLearningProgress>()
                .WithTracking(false)
                .WithPredicate(x => x.UserId == userId && x.LearningContentId == learningContentId)
                .Build();
            var data = await _unitOfWork.GetRepo<UserLearningProgress>().GetSingleAsync(queryOtion);
            return data;
        }

        //public async Task<UserLearningProgress> MarkAsCompleteLearningProgress(long userId,  long learningContentId)
        //{

        //}
    }
}
