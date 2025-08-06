using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IChallengeSessionRepository
    {
        public Task<ChallengeSession> CreateChallengeSessionAsync(ChallengeSession challengeSession);
        public Task<List<ChallengeSession>> GetAllChallengeSessionsByUserIdAsync(long userId);
        public Task<List<ChallengeSession>> GetChallengeSessionByChallengeIdAsync(long challengeId);
        public Task DeleteAllChallengeSessionsAsync(List<ChallengeSession> challengeSessions);
    }
}
