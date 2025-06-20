using Helpers.DTOs.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Courses
{
    public class CourseHasEnrolledBasicViewReponseDTO
    {
        public long CourseId { get; set; }
        public string? CourseTitle { get; set; }
        public string? CourseDescription { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? RegionName { get; set; }
        public string? HistorialPeriodName { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<ModuleCourseHasEnrolledBasicViewDTO> Modules { get; set; } = new List<ModuleCourseHasEnrolledBasicViewDTO>();

    }
}
