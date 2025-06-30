using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Question
{
    public class GetQuestionQuizDTO
    {
        public long QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public decimal Points { get; set; } = 0;
        public int OrderIndex { get; set; }
        public List<GetOptionQuizDTO> QuestionOptions { get; set; } = new List<GetOptionQuizDTO>();
    }
}
