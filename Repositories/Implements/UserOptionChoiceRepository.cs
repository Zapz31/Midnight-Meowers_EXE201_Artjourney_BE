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
    public class UserOptionChoiceRepository : IUserOptionChoiceRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public UserOptionChoiceRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<List<UserOptionChoice>> GetUserChoicesByUserIdAsync(long userId)
        {
            var queryOption = new QueryBuilder<UserOptionChoice>()
                .WithTracking(false)
                .WithPredicate(uoc => uoc.UserId == userId)
                .WithInclude(uoc => uoc.SurveyOption)
                .WithInclude(uoc => uoc.SurveyOption.SurveyQuestion)
                .Build();
            
            var data = await _unitOfWork.GetRepo<UserOptionChoice>().GetAllAsync(queryOption);
            return data.ToList();
        }

        public async Task<UserOptionChoice?> GetUserChoiceByUserAndOptionAsync(long userId, long surveyOptionId)
        {
            var queryOption = new QueryBuilder<UserOptionChoice>()
                .WithTracking(false)
                .WithPredicate(uoc => uoc.UserId == userId && uoc.SurveyOptionId == surveyOptionId)
                .Build();
            
            var data = await _unitOfWork.GetRepo<UserOptionChoice>().GetSingleAsync(queryOption);
            return data;
        }

        public async Task<UserOptionChoice> CreateUserOptionChoiceAsync(UserOptionChoice userOptionChoice)
        {
            var createdUserOptionChoice = await _unitOfWork.GetRepo<UserOptionChoice>().CreateAsync(userOptionChoice);
            await _unitOfWork.SaveChangesAsync();
            return createdUserOptionChoice;
        }

        public async Task UpdateUserOptionChoiceAsync(UserOptionChoice userOptionChoice)
        {
            await _unitOfWork.GetRepo<UserOptionChoice>().UpdateAsync(userOptionChoice);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteUserOptionChoiceAsync(long userOptionChoiceId)
        {
            var userOptionChoice = await _context.UserOptionChoices.FindAsync(userOptionChoiceId);
            if (userOptionChoice != null)
            {
                await _unitOfWork.GetRepo<UserOptionChoice>().DeleteAsync(userOptionChoice);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<List<UserOptionChoice>> CreateMultipleUserOptionChoicesAsync(List<UserOptionChoice> userOptionChoices)
        {
            var createdChoices = new List<UserOptionChoice>();
            foreach (var choice in userOptionChoices)
            {
                var createdChoice = await _unitOfWork.GetRepo<UserOptionChoice>().CreateAsync(choice);
                createdChoices.Add(createdChoice);
            }
            await _unitOfWork.SaveChangesAsync();
            return createdChoices;
        }

        public async Task<bool> HasUserCompletedSurveyAsync(long userId)
        {
            return await _context.UserOptionChoices
                .AsNoTracking()
                .AnyAsync(uoc => uoc.UserId == userId);
        }

        public async Task DeleteAllUserChoicesAsync(long userId)
        {
            var userChoices = await _context.UserOptionChoices
                .Where(uoc => uoc.UserId == userId)
                .ToListAsync();

            if (userChoices.Any())
            {
                _context.UserOptionChoices.RemoveRange(userChoices);
                await _context.SaveChangesAsync();
            }
        }
    }
}
