using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("course_reviews")]
    public class CourseReview
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("course_review_id")]
        public long CourseReviewId { get; set; }
        [Column("user_id")]
        public long UserId { get; set; }
        [Column("course_id")]
        public long CourseId { get; set; }
        [Column("rating")]
        public int Rating { get; set; } = 5;
        [Column("feedback")]
        public string? Feedback { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        [Column("is_approved")]
        public bool IsApproved { get; set; } = true;
    }
}
