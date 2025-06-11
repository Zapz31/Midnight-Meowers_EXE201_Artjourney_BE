using BusinessObjects.Models;
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
    }
}
