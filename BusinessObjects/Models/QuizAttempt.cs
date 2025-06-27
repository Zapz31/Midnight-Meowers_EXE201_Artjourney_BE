using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("quiz_attempts")]
    public class QuizAttempt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("id")]
        public long Id { get; set; }

        [Column("started_at")]
        public DateTime? StartedAt { get; set; }

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("total_score", TypeName = "decimal(4,1)")]
        public decimal TotalScore { get; set; } = 0;

        [Column("total_possible_score", TypeName = "decimal(4,1)")]
        public decimal TotalPossibleScore { get; set; } = 0;

        [Column("is_completed")]
        public bool IsCompleted { get; set; } = false;

        [Column("time_taken")]
        public TimeSpan? TimeTaken { get; set; }

        [Column("learning_content_id")]
        [ForeignKey("LearningContent")]
        public long LearningContentId { get; set; }
        public virtual LearningContent LearningContent { get; set; } = null!;

        [Column("user_id")]
        [ForeignKey("User")]
        public long UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [InverseProperty("QuizAttempt")]
        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}
