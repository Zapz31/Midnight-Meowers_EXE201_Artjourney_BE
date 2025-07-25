using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Survey
{
    public class CreateSurveyRequestDTO
    {
        public string SurveyQuestionContent { get; set; } = string.Empty;
        public List<CreateSurveyOptionDTO> Options { get; set; } = new List<CreateSurveyOptionDTO>();
    }

    public class CreateSurveyOptionDTO
    {
        public string SurveyOptionContent { get; set; } = string.Empty;
    }
}
