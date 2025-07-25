using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ISurveyQuestionRepository
    {
        Task<List<SurveyQuestion>> GetAllActiveSurveyQuestionsAsync();
        Task<List<SurveyQuestion>> GetAllActiveSurveyQuestionsWithOptionsAsync();
        Task<SurveyQuestion?> GetSurveyQuestionByIdAsync(long surveyQuestionId);
        Task<SurveyQuestion> CreateSurveyQuestionAsync(SurveyQuestion surveyQuestion);
        Task UpdateSurveyQuestionAsync(SurveyQuestion surveyQuestion);
        Task DeleteSurveyQuestionAsync(long surveyQuestionId);
        Task<List<SurveyQuestion>> GetAllSurveyQuestionsWithOptionsAsync();
        Task<SurveyQuestion?> GetSurveyQuestionWithOptionsAsync(long surveyQuestionId);
    }
}
