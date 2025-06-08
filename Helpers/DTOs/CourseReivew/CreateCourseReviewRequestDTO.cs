using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.CourseReivew
{
    public class CreateCourseReviewRequestDTO
    {
        public long CourseId { get; set; }
        public int Rating { get; set; } = 5;
        public string? FeedBack { get; set; }
    }
}
