using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Courses
{
    public class SearchResultCourseDTO
    {
        public long CourseId { get; set; }
        public string? Title { get; set; }
        public string? ThumbnailUrl { get; set; }
        public decimal AverageRating { get; set; } = 0;
        public int TotalFeedbacks { get; set; } = 0;

    }
}
