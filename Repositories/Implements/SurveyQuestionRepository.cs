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
    public class SurveyQuestionRepository : ISurveyQuestionRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public SurveyQuestionRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

    public async Task<List<SurveyQuestion>> GetAllActiveSurveyQuestionsAsync()
    {
        var options = new QueryBuilder<SurveyQuestion>()
            .WithTracking(false)
            .WithPredicate(sq => sq.IsActive)
            .WithOrderBy(query => query.OrderBy(sq => sq.SurveyQuestionId))
            .Build();

        var questions = await _unitOfWork.GetRepo<SurveyQuestion>().GetAllAsync(options);
        return questions.ToList();
    }

    public async Task<List<SurveyQuestion>> GetAllActiveSurveyQuestionsWithOptionsAsync()
    {
        return await _context.SurveyQuestions
            .AsNoTracking()
            .Include(sq => sq.SurveyOptions.Where(so => so.IsAcive))
            .Include(sq => sq.CreatedByUser)
            .Where(sq => sq.IsActive)
            .OrderBy(sq => sq.SurveyQuestionId)
            .ToListAsync();
    }        public async Task<SurveyQuestion?> GetSurveyQuestionByIdAsync(long surveyQuestionId)
        {
            var queryOption = new QueryBuilder<SurveyQuestion>()
                .WithTracking(false)
                .WithPredicate(sq => sq.SurveyQuestionId == surveyQuestionId)
                .Build();
            
            var data = await _unitOfWork.GetRepo<SurveyQuestion>().GetSingleAsync(queryOption);
            return data;
        }

        public async Task<SurveyQuestion> CreateSurveyQuestionAsync(SurveyQuestion surveyQuestion)
        {
            var createdSurveyQuestion = await _unitOfWork.GetRepo<SurveyQuestion>().CreateAsync(surveyQuestion);
            await _unitOfWork.SaveChangesAsync();
            return createdSurveyQuestion;
        }

        public async Task UpdateSurveyQuestionAsync(SurveyQuestion surveyQuestion)
        {
            await _unitOfWork.GetRepo<SurveyQuestion>().UpdateAsync(surveyQuestion);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteSurveyQuestionAsync(long surveyQuestionId)
        {
            var surveyQuestion = await GetSurveyQuestionByIdAsync(surveyQuestionId);
            if (surveyQuestion != null)
            {
                await _unitOfWork.GetRepo<SurveyQuestion>().DeleteAsync(surveyQuestion);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<List<SurveyQuestion>> GetAllSurveyQuestionsWithOptionsAsync()
        {
            return await _context.SurveyQuestions
                .AsNoTracking()
                .Include(sq => sq.SurveyOptions)
                .Include(sq => sq.CreatedByUser)
                .OrderByDescending(sq => sq.CreatedAt)
                .ToListAsync();
        }

        public async Task<SurveyQuestion?> GetSurveyQuestionWithOptionsAsync(long surveyQuestionId)
        {
            return await _context.SurveyQuestions
                .AsNoTracking()
                .Include(sq => sq.SurveyOptions)
                .Include(sq => sq.CreatedByUser)
                .FirstOrDefaultAsync(sq => sq.SurveyQuestionId == surveyQuestionId);
        }
    }
}
