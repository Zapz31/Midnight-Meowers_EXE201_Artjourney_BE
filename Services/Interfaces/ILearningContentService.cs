using BusinessObjects.Models;
using Helpers.DTOs.ChallengeItem;
using Helpers.DTOs.LearningContent;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ILearningContentService
    {
        public Task<ApiResponse<LearningContent>> CreateLNContentReadingAsync(CreateLNReadingDTO requestDTO);
        public Task<ApiResponse<List<BasicLearningContentGetResponseDTO>>> GetLearningContentsBySubmoduleId(long subModuleId);
        public Task<ApiResponse<List<BasicChallengeItemGetResponseDTO>>> GetChallengeItemsByLNCId(long learningContentId);
        public Task<ApiResponse<QuizAttempt>> StartQuizAsync(long userId, long learningContentId);
        public Task<ApiResponse<bool>> SubmitQuizAsync(SubmitQuizRequestDTO submitQuizRequestDTO);
        public Task<ApiResponse<LearningContent>> CreateQuizTitle(CreateQuizTitleRequestDTO createQuizTitleRequestDTO);
        public  Task<ApiResponse<int>> SoftDeleteLearningContentAsync(long learningContentIdInput);

    }
}
