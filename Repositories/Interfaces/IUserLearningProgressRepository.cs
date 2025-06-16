using BusinessObjects.Enums;
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IUserLearningProgressRepository
    {
        public Task<UserLearningProgress> CreateUserLearningProgressAsync(UserLearningProgress userLearningProgress);
        public Task<UserLearningProgress?> GetUserLearningProgressByUserIdAndLNCId(long userId, long learningContentId);
        public Task<List<long>> GetExistLCIdsByUserId(long userId, List<long> learningContentIdsByCourseIds, UserLearningProgressStatus? status);
        public Task CreateAllUserLearningProgressAsync(List<UserLearningProgress> userLearningProgresses);
        public Task<IEnumerable<UserLearningProgress>> GetLearningProgressByUserIdAndLNCIds(long userId, List<long> learningContentIds);
        public Task UpdateUserLearningProgressSingleAsync(UserLearningProgress userLearningProgress);
        public Task<UserLearningProgress?> GetLearningProgressByUserIdAndLNCIdSingle(long userId, long learningContentId);
        public Task CreateUserLearningProgressesAsync(List<UserLearningProgress> userLearningProgresses);
    }
}
