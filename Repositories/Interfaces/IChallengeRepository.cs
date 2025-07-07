using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IChallengeRepository
    {
        public Task<List<Challenge>> GetAllChallengesByCourseIdAsync(long courseId);
        public Task CreateChallengesAsync(List<Challenge> challenges);
        public Task<Challenge?> GetChallengeByIdAsync(long challengeId);
    }
}
