using Helpers.DTOs.LearningContent;
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
    }
}
