using Helpers.DTOs.HistoricalPeriod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Courses
{
    public class LearnPageCourseReginDTO
    {
        public long? RegionId { get; set; }

        public List<HistoricalPeriodDTO> historicalPeriodDTOs { get; set; } = new List<HistoricalPeriodDTO>();
        public string? RegionName { get; set; }
        public List<CourseDTO> Courses { get; set; } = new List<CourseDTO>();
    }
}
