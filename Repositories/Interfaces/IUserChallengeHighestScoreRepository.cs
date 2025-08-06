using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IUserChallengeHighestScoreRepository
    {
        public Task<UserChallengeHighestScore?> GetHighestScoreByUserIdAndChallengeId(long userId, long challengeId);
        public Task<UserChallengeHighestScore> CreateUserChallengeHighestScoreAsync(UserChallengeHighestScore userChallengeHighestScore);
        public Task UpdateUserChallengeHighestScoreAsync(UserChallengeHighestScore userChallengeHighestScore);
        public Task<List<UserChallengeHighestScore>> GetChallengeLearboardAsync(long challengeId);
        public Task<List<UserChallengeHighestScore>> GetAllUserChallengeHighestScoresByChallengeIdAsync(long challengeId);
        public Task<List<UserChallengeHighestScore>> GetAllUserChallengeHighestScoresByUserIdAsync(long userId);
        public Task DeleteAllUserChallengeHighestScoresAsync(List<UserChallengeHighestScore> userChallengeHighestScores);
        
    }
}
