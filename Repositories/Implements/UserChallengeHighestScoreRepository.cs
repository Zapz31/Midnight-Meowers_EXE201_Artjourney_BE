using BusinessObjects.Models;
using DAOs;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class UserChallengeHighestScoreRepository : IUserChallengeHighestScoreRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public UserChallengeHighestScoreRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<UserChallengeHighestScore?> GetHighestScoreByUserIdAndChallengeId(long userId, long challengeId)
        {
            var queryOption = new QueryBuilder<UserChallengeHighestScore>()
                .WithTracking(false)
                .WithPredicate(u => u.UserId == userId && u.ChallengeId == challengeId)
                .Build();
            var data = await _unitOfWork.GetRepo<UserChallengeHighestScore>().GetSingleAsync(queryOption);
            return data;
        }

        public async Task<UserChallengeHighestScore> CreateUserChallengeHighestScoreAsync(UserChallengeHighestScore userChallengeHighestScore)
        {
            var createdUserChallengeHighestScore = await _unitOfWork.GetRepo<UserChallengeHighestScore>().CreateAsync(userChallengeHighestScore);
            await _unitOfWork.SaveChangesAsync();
            return createdUserChallengeHighestScore;
        }

        public async Task UpdateUserChallengeHighestScoreAsync(UserChallengeHighestScore userChallengeHighestScore)
        {
            await _unitOfWork.GetRepo<UserChallengeHighestScore>().UpdateAsync(userChallengeHighestScore);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<UserChallengeHighestScore>> GetChallengeLearboardAsync(long challengeId)
        {
            var leaderboard = await _context.UserChallengeHighestScores
                    .AsNoTracking()
                    .Where(x => x.ChallengeId == challengeId)
                    .Include(x => x.User)
                    .OrderByDescending(x => x.HighestScore)
                    .ThenBy(x => x.TimeTaken)
                    .ToListAsync();
            return leaderboard;
        }

        public async Task<List<UserChallengeHighestScore>> GetAllUserChallengeHighestScoresByChallengeIdAsync(long challengeId)
        {
            var queryOption = new QueryBuilder<UserChallengeHighestScore>()
                .WithTracking(false)
                .WithPredicate(u => u.ChallengeId == challengeId)
                .Build();
            var data = await _unitOfWork.GetRepo<UserChallengeHighestScore>().GetAllAsync(queryOption);
            return data.ToList();
        }

        public async Task<List<UserChallengeHighestScore>> GetAllUserChallengeHighestScoresByUserIdAsync(long userId)
        {
            var queryOption = new QueryBuilder<UserChallengeHighestScore>()
                .WithTracking(false)
                .WithPredicate(u => u.UserId == userId)
                .Build();
            var data = await _unitOfWork.GetRepo<UserChallengeHighestScore>().GetAllAsync(queryOption);
            return data.ToList();
        }
        
        public async Task DeleteAllUserChallengeHighestScoresAsync(List<UserChallengeHighestScore> userChallengeHighestScores)
        {
            await _unitOfWork.GetRepo<UserChallengeHighestScore>().DeleteAllAsync(userChallengeHighestScores);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
