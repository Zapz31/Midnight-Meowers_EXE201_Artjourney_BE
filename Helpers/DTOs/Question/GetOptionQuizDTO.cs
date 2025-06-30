using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Question
{
    public class GetOptionQuizDTO
    {
        public long QuestionOptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public int OrderIndex { get; set; }

    }
}
