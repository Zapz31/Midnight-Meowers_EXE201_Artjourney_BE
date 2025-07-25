using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Survey
{
    public class SimpleSurveyQuestionResponseDTO
    {
        public long SurveyQuestionId { get; set; }
        public string SurveyQuestionContent { get; set; } = string.Empty;
        public List<SimpleSurveyOptionResponseDTO> Options { get; set; } = new List<SimpleSurveyOptionResponseDTO>();
    }

    public class SimpleSurveyOptionResponseDTO
    {
        public long SurveyOptionId { get; set; }
        public string SurveyOptionContent { get; set; } = string.Empty;
    }
}
