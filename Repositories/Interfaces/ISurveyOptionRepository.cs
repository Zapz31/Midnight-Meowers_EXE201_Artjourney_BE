using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ISurveyOptionRepository
    {
        Task<List<SurveyOption>> GetOptionsByQuestionIdAsync(long surveyQuestionId);
        Task<SurveyOption?> GetSurveyOptionByIdAsync(long surveyOptionId);
        Task<SurveyOption> CreateSurveyOptionAsync(SurveyOption surveyOption);
        Task UpdateSurveyOptionAsync(SurveyOption surveyOption);
        Task DeleteSurveyOptionAsync(long surveyOptionId);
        Task<List<SurveyOption>> CreateMultipleSurveyOptionsAsync(List<SurveyOption> surveyOptions);
    }
}
