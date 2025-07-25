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
    public class SurveyOptionRepository : ISurveyOptionRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public SurveyOptionRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<List<SurveyOption>> GetOptionsByQuestionIdAsync(long surveyQuestionId)
        {
            var queryOption = new QueryBuilder<SurveyOption>()
                .WithTracking(false)
                .WithPredicate(so => so.SurveyQuestionId == surveyQuestionId && so.IsAcive)
                .Build();
            
            var data = await _unitOfWork.GetRepo<SurveyOption>().GetAllAsync(queryOption);
            return data.ToList();
        }

        public async Task<SurveyOption?> GetSurveyOptionByIdAsync(long surveyOptionId)
        {
            var queryOption = new QueryBuilder<SurveyOption>()
                .WithTracking(false)
                .WithPredicate(so => so.SurveyOptionId == surveyOptionId)
                .Build();
            
            var data = await _unitOfWork.GetRepo<SurveyOption>().GetSingleAsync(queryOption);
            return data;
        }

        public async Task<SurveyOption> CreateSurveyOptionAsync(SurveyOption surveyOption)
        {
            var createdSurveyOption = await _unitOfWork.GetRepo<SurveyOption>().CreateAsync(surveyOption);
            await _unitOfWork.SaveChangesAsync();
            return createdSurveyOption;
        }

        public async Task UpdateSurveyOptionAsync(SurveyOption surveyOption)
        {
            await _unitOfWork.GetRepo<SurveyOption>().UpdateAsync(surveyOption);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteSurveyOptionAsync(long surveyOptionId)
        {
            var surveyOption = await GetSurveyOptionByIdAsync(surveyOptionId);
            if (surveyOption != null)
            {
                await _unitOfWork.GetRepo<SurveyOption>().DeleteAsync(surveyOption);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<List<SurveyOption>> CreateMultipleSurveyOptionsAsync(List<SurveyOption> surveyOptions)
        {
            var createdOptions = new List<SurveyOption>();
            foreach (var option in surveyOptions)
            {
                var createdOption = await _unitOfWork.GetRepo<SurveyOption>().CreateAsync(option);
                createdOptions.Add(createdOption);
            }
            await _unitOfWork.SaveChangesAsync();
            return createdOptions;
        }
    }
}
