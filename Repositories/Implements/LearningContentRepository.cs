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
    public class LearningContentRepository : ILearningContentRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        public LearningContentRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<LearningContent> CreateLearningContentAsync(LearningContent learningContent)
        {
            var createdLearningContent = await _unitOfWork.GetRepo<LearningContent>().CreateAsync(learningContent);
            await _unitOfWork.SaveChangesAsync();
            return createdLearningContent;
        }

        public async Task<ChallengeItem> CreateChallengeItemAsync(ChallengeItem challengeItem)
        {
            var createdChallengeItem = await _unitOfWork.GetRepo<ChallengeItem>().CreateAsync(challengeItem);
            await _unitOfWork.SaveChangesAsync();
            return createdChallengeItem;
        }

        public async Task<bool> CreateAllChallengeItemAsync(List<ChallengeItem> challengeItems)
        {
            await _unitOfWork.GetRepo<ChallengeItem>().CreateAllAsync(challengeItems);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<long>> GetLearningContentContentIdsByCourseIdAsync(long coursId)
        {
            var options = new QueryBuilder<LearningContent>()
                .WithTracking(false)
                .WithPredicate(lc => lc.CourseId == coursId && lc.IsActive == true)
                .Build();
            var queryData = await _unitOfWork.GetRepo<LearningContent>().GetAllAsync(options);
            var learningContentIdsByCourseIds = queryData.Select(queryData => queryData.LearningContentId);
            return learningContentIdsByCourseIds.ToList();
        }

        public async Task<IEnumerable<LearningContent>> GetLearningContentsBySubmoduleIds(List<long> subModuleIds)
        {
            var learningContentQuery = new QueryBuilder<LearningContent>()
                .WithPredicate(lc => subModuleIds.Contains(lc.SubModuleId) && lc.IsActive == true)
                .WithOrderBy(query => query.OrderBy(lc => lc.DisplayOrder))
                .Build();

            var learningContents = await _unitOfWork.GetRepo<LearningContent>().GetAllAsync(learningContentQuery);
            return learningContents;
        }

    }
}
