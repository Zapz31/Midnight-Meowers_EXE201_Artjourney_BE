using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.LearningContent
{
    public class SubmitQuizRequestDTO
    {
        public long LearningContentId { get; set; }
        public long QuizAttemptId { get; set; }
        public List<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}
