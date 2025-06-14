using BusinessObjects.Enums;
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.UserCourseInfo
{
    public class BasicUserCourseInfoGetReponseDTO
    {
        public long InfoId { get; set; }
        public CourseEnrollmentStatus EnrollmentStatus { get; set; } = CourseEnrollmentStatus.Enrolled;
        public CourseLearningStatus LearningStatus { get; set; } = CourseLearningStatus.NotStarted;
        public DateTime? CompletedAt { get; set; }
        public DateTime? EnrolledAt { get; set; } = DateTime.UtcNow;
        public TimeSpan? CompletedIn { get; set; }
        public decimal ProgressPercent { get; set; } = 0;
        public long UserId { get; set; }
        public long CourseId { get; set; }
     
    }
}
