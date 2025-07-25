using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Survey
{
    public class UpdateSurveyRequestDTO
    {
        public long SurveyQuestionId { get; set; }
        public string SurveyQuestionContent { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public List<UpdateSurveyOptionDTO> Options { get; set; } = new List<UpdateSurveyOptionDTO>();
    }

    public class UpdateSurveyOptionDTO
    {
        public long? SurveyOptionId { get; set; } // Null for new options
        public string SurveyOptionContent { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false; // For marking options to delete
    }
}
