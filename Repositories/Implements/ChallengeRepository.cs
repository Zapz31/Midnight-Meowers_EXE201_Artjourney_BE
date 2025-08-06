using BusinessObjects.Models;
using DAOs;
using Microsoft.AspNetCore.Http.Extensions;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class ChallengeRepository : IChallengeRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public ChallengeRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<List<Challenge>> GetAllChallengesByCourseIdAsync(long courseId)
        {
            var queryOption = new QueryBuilder<Challenge>()
                .WithTracking(false)
                .WithPredicate(x =>  x.CourseId == courseId)
                .Build();

            var data = await _unitOfWork.GetRepo<Challenge>().GetAllAsync(queryOption);
            return data.ToList();
        }

        public async Task CreateChallengesAsync(List<Challenge> challenges)
        {
            await _unitOfWork.GetRepo<Challenge>().CreateAllAsync(challenges);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Challenge?> GetChallengeByIdAsync(long challengeId)
        {
            var queryOption = new QueryBuilder<Challenge>()
                .WithTracking(false)
                .WithPredicate(c => c.Id == challengeId)
                .Build();
            var data = await _unitOfWork.GetRepo<Challenge>().GetSingleAsync(queryOption);
            return data;
        }

        public async Task DeleteChallengeById(Challenge challenge)
        {
            await _unitOfWork.GetRepo<Challenge>().DeleteAsync(challenge);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
