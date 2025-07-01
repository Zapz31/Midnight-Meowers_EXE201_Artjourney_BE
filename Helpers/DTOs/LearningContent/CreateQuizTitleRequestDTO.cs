using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.LearningContent
{
    public class CreateQuizTitleRequestDTO
    {
        public LearningContentType? ContentType { get; set; }
        public string? Title { get; set; }
        public TimeSpan? TimeLimit { get; set; }
        public int DisplayOrder { get; set; }
        public long SubModuleId { get; set; }
        public long? CourseId { get; set; }

    }
}
