using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.LearningContent
{
    public class CreateQuestionsAndOptionsBasicRequestDTO
    {
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty; // SingleChoice or MultipleChoice
        public decimal Points { get; set; } = 0;
        public int OrderIndex { get; set; }
        public long LearningContentId { get; set; }
        public List<CreateOptionBasicRequestDTO> QuestionOptions { get; set; } = new List<CreateOptionBasicRequestDTO>();

    }
}
