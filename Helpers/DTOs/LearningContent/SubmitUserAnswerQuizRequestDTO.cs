using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.LearningContent
{
    public class SubmitUserAnswerQuizRequestDTO
    {
        public long QuizAttemptId { get; set; }
        public long QuestionId { get; set; }
        public long SelectedOptionId { get; set; }

    }
}
