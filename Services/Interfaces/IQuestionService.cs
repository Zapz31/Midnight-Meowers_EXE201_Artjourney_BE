using Helpers.DTOs.LearningContent;
using Helpers.DTOs.Question;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IQuestionService
    {
        public Task<ApiResponse<bool>> CreateQuestionsAndOptionsAsync(List<CreateQuestionsAndOptionsBasicRequestDTO> CreateQuestionsAndOptionsBasicRequestDTOs);
        public Task<ApiResponse<PaginatedResult<GetQuestionQuizDTO>>> GetQuestionWithOptionQuizAsync(long learningContentId, int pageNumber, int pageSize);
    }
}
