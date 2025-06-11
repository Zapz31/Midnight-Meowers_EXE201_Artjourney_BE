using BusinessObjects.Enums;
using Helpers.DTOs.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Courses
{
    public class CourseDetailResponseDTO
    {
        public long CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public int TotalModule { get; set; } = 0;
        public CourseLevel CourseLevel { get; set; }
        public string? LearningOutcomes { get; set; }
        public List<ModuleCourseDetailScreenResponseDTO> ModuleCourseDetailScreenResponseDTOs { get; set; } = new List<ModuleCourseDetailScreenResponseDTO>();
        public int CourseCompletionPercentage { get; set; } = 0;
        public int TimeSpentPercentage { get; set; } = 0;
        public TimeSpan RemainingTime { get; set; } = TimeSpan.Zero;

    }
}
