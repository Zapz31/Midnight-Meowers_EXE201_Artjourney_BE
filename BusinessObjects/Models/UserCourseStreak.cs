using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("user_course_streaks")]
    public class UserCourseStreak
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("id")]
        public long Id { get; set; }
        [Column("user_id")]
        public long UserId { get; set; }

        [Column("course_id")]
        public long CourseId { get; set; }

        [Column("current_streak")]
        public int CurrentStreak { get; set; }

        [Column("longest_streak")]
        public int LongestStreak { get; set; }

        [Column("total_days_accessed")]
        public int TotalDaysAccessed { get; set; }

        [Column("last_access_date")]
        public DateOnly LastAccessDate { get; set; }
    }
}
