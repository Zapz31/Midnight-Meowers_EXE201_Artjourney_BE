using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("user_course_infos")]
    public class UserCourseInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("info_id")]
        public long InfoId { get; set; }

        [Column("enrollment_status")]
        public CourseEnrollmentStatus EnrollmentStatus { get; set; } = CourseEnrollmentStatus.Enrolled;

        [Column("learning_status")]
        public CourseLearningStatus LearningStatus { get; set; } = CourseLearningStatus.NotStarted;

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("enrolled_at")]
        public DateTime? EnrolledAt { get; set; } = DateTime.UtcNow;

        [Column("completed_in")]
        public TimeSpan? CompletedIn {  get; set; }

        [Column("progress_percent", TypeName = "decimal(5,2)")]
        public decimal ProgressPercent { get; set; } = 0;

        [Column("streak")]
        public int Streak { get; set; } = 0;

        // Navigation properties
        // N - 1
        [Column("user_id")]
        [ForeignKey("User")]
        public long UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Column("course_id")]
        public long CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;
    }
}

/*
table user_course_info {
  info_id long [primary key, increment]
  enrollment_status varchar //-- enrolled, refunded [enum]
  learning_status varchar // not_started, in_progress, completed, suspended [enum] ==
  completed_at datetime ==
  enrolled_at datetime ==
  completed_in long // hoàn thành khóa học trong bao lâu, tính bằng giây ==
  progress_percent decimal(5,2) ==
  streak int // ghi nhận daily streak của người học ==
  user_id long [ref: > users.id] ==
  course_id long [ref: > courses.course_id]
}
}
 */