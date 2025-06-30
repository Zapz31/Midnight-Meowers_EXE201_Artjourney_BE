using BusinessObjects.Models;
using Helpers.DTOs.LearningContent;
using Helpers.DTOs.Question;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IQuestionRepository
    {
        public Task CreateQuestionsAsync(List<Question> questions);
        public Task<bool> CreateQuestionsWithOptionsBulkAsync(List<CreateQuestionsAndOptionsBasicRequestDTO> requestDTOs);
        public Task<PaginatedResult<GetQuestionQuizDTO>> GetQuestionWithOptionQuizAsync(long learningContentId, int pageNumber, int pageSize);
    }
}
