using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Survey
{
    public class UserSurveySubmissionDTO
    {
        public List<UserSurveyAnswerDTO> Answers { get; set; } = new List<UserSurveyAnswerDTO>();
    }

    public class UserSurveyAnswerDTO
    {
        public long SurveyOptionId { get; set; }
        public string? Content { get; set; } // Optional additional content/comment from user
    }
}
