using BusinessObjects.Models;
using DAOs;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class ChallengeSessionRepository : IChallengeSessionRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public ChallengeSessionRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            this._unitOfWork = unitOfWork;
            this._context = context;
        }

        public async Task<ChallengeSession> CreateChallengeSessionAsync(ChallengeSession challengeSession)
        {
            var createChallengeSession = await _unitOfWork.GetRepo<ChallengeSession>().CreateAsync(challengeSession);
            await _unitOfWork.SaveChangesAsync();
            return createChallengeSession;
        }

        public async Task<List<ChallengeSession>> GetAllChallengeSessionsByUserIdAsync(long userId)
        {
            var queryOption = new QueryBuilder<ChallengeSession>()
                .WithTracking(false)
                .WithPredicate(cs => cs.UserId == userId)
                .Build();

            var data = await _unitOfWork.GetRepo<ChallengeSession>().GetAllAsync(queryOption);
            return data.ToList();
        }

        public async Task<List<ChallengeSession>> GetChallengeSessionByChallengeIdAsync(long challengeId)
        {
            var queryOption = new QueryBuilder<ChallengeSession>()
                .WithTracking(false)
                .WithPredicate(cs => cs.ChallengeId == challengeId)
                .Build();

            var data = await _unitOfWork.GetRepo<ChallengeSession>().GetAllAsync(queryOption);
            return data.ToList();
        }

        public async Task DeleteAllChallengeSessionsAsync(List<ChallengeSession> challengeSessions)
        {
            await _unitOfWork.GetRepo<ChallengeSession>().DeleteAllAsync(challengeSessions);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
