using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Courses
{
    [Keyless]
    public class CourseDetailScreenFlat
    {
        public long ModuleId { get; set; }
        public string ModuleTitle { get; set; } = string.Empty;
        public long SubModuleId { get; set; }
        public string? SubModuleTitle { get; set;}
        public long LearningContentId { get; set; }
        public string? Title { get; set; }
        public int DisplayOrder { get; set; }
        public TimeSpan? TimeLimit { get; set; }
    }
}
