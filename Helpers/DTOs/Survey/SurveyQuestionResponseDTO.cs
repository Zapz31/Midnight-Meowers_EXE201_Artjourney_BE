using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Survey
{
    public class SurveyQuestionResponseDTO
    {
        public long SurveyQuestionId { get; set; }
        public string SurveyQuestionContent { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public List<SurveyOptionResponseDTO> Options { get; set; } = new List<SurveyOptionResponseDTO>();
    }

    public class SurveyOptionResponseDTO
    {
        public long SurveyOptionId { get; set; }
        public string SurveyOptionContent { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public long SurveyQuestionId { get; set; }
    }
}
