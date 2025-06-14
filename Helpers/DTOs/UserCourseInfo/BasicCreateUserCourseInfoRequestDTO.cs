using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.UserCourseInfo
{
    public class BasicCreateUserCourseInfoRequestDTO
    {
        public CourseEnrollmentStatus EnrollmentStatus { get; set; } = CourseEnrollmentStatus.Enrolled;
        public CourseLearningStatus LearningStatus { get; set; } = CourseLearningStatus.NotStarted;
        public long UserId { get; set; }
        public long CourseId { get; set; }
    }
}
