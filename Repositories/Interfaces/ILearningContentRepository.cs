using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ILearningContentRepository
    {
        public Task<LearningContent> CreateLearningContentAsync(LearningContent learningContent);
        public Task<ChallengeItem> CreateChallengeItemAsync(ChallengeItem challengeItem);
        public Task<bool> CreateAllChallengeItemAsync(List<ChallengeItem> challengeItems);
        public Task<List<long>> GetLearningContentContentIdsByCourseIdAsync(long coursId);
        public Task<IEnumerable<LearningContent>> GetLearningContentsBySubmoduleIds(List<long> subModuleIds);
    }
}
