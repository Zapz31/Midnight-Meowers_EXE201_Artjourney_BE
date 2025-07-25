using Helpers.DTOs.Survey;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ISurveyService
    {
        // Admin operations
        Task<ApiResponse<SurveyQuestionResponseDTO>> CreateSurveyQuestionAsync(CreateSurveyRequestDTO createSurveyRequest);
        Task<ApiResponse<List<SurveyQuestionResponseDTO>>> CreateMultipleSurveyQuestionsAsync(CreateMultipleSurveysRequestDTO createMultipleSurveysRequest);
        Task<ApiResponse<SurveyQuestionResponseDTO>> UpdateSurveyQuestionAsync(UpdateSurveyRequestDTO updateSurveyRequest);
        Task<ApiResponse<bool>> DeleteSurveyQuestionAsync(long surveyQuestionId);
        Task<ApiResponse<List<SurveyQuestionResponseDTO>>> GetAllSurveyQuestionsAsync();
        Task<ApiResponse<SurveyQuestionResponseDTO>> GetSurveyQuestionByIdAsync(long surveyQuestionId);

        // User operations
        Task<ApiResponse<List<SurveyQuestionResponseDTO>>> GetActiveSurveyQuestionsForUserAsync();
        Task<ApiResponse<List<SimpleSurveyQuestionResponseDTO>>> GetActiveSurveyQuestionsSimpleAsync();
        Task<ApiResponse<string>> SubmitUserSurveyAsync(UserSurveySubmissionDTO userSurveySubmission);
        Task<ApiResponse<List<SurveyQuestionResponseDTO>>> GetUserSurveyHistoryAsync();
        Task<ApiResponse<bool>> CheckUserSurveyStatusAsync();
    }
}
