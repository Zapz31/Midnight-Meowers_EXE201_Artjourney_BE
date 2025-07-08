using BusinessObjects.Models;
using DAOs;
using Repositories.Interfaces;
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


    }
}
