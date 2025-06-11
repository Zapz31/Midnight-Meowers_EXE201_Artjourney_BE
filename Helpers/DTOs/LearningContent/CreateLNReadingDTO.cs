using BusinessObjects.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.LearningContent
{
    public class CreateLNReadingDTO
    {
        [Required]
        public string? Title { get; set; }
        public string? Content { get; set; }
        public IFormFile? Video { get; set; }
        public TimeSpan? TimeLimit { get; set; }
        public int DisplayOrder { get; set; }
        public long SubModuleId { get; set; }
        public long CourseId { get; set; }

    }
}
