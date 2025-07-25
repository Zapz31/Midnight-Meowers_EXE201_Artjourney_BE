using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Survey
{
    public class CreateMultipleSurveysRequestDTO
    {
        public List<CreateSurveyRequestDTO> SurveyQuestions { get; set; } = new List<CreateSurveyRequestDTO>();
    }
}
