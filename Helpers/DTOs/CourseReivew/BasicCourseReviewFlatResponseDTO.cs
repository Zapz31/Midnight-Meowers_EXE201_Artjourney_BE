using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.CourseReivew
{
    [Keyless]
    public class BasicCourseReviewFlatResponseDTO
    {
        // From users (u)
        public string FullName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;

        // From course_reviews (cr)
        public long CourseReviewId { get; set; }
        public long UserId { get; set; }
        public long CourseId { get; set; }
        public int Rating { get; set; }
        public string? Feedback { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsApproved { get; set; } = true;
    }
}
