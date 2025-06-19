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
    [Table("user_learning_progresses")]
    public class UserLearningProgress
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("progress_id")]
        public long ProgressId { get; set; }

        [Column("status")]
        public UserLearningProgressStatus Status { get; set; } = UserLearningProgressStatus.NotStarted;

        [Column("score", TypeName = "decimal(4,1)")]
        public decimal Score { get; set; } = 0;

        [Column("completion_time")]
        public TimeSpan CompletionTime = TimeSpan.Zero;

        [Column("attempts")]
        public int Attempts { get; set; } = 0;

        [Column("last_attempt_at")]
        public DateTime? LastAttemptAt { get; set; }

        [Column("started_at")]
        public DateTime? StartedAt { get; set; }

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("completed_in")]
        public TimeSpan? CompletedIn { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }


        // Navigation Properties
        // N - 1
        [Column("user_id")]
        [ForeignKey("User")]
        public long UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Column("learning_content_id")]
        [ForeignKey("LearningContent")]
        public long LearningContentId { get; set; }
        public virtual LearningContent LearningContent { get; set; } = null!;

    }
}

/*
[Column("complete_criteria", TypeName = "decimal(4,1)")]
        public decimal? CompleteCriteria { get; set; }
  
 Table user_progress {
    progress_id long [primary key, increment] ==
    user_id long [ref: > users.id] - PK == 
    learning_content_id long [ref: > learning_contents.learning_content_id] - PK ==
    status varchar // -- ENUM('not_started', 'in_progress', 'completed') - enum ==
    score FLOAT //-- Điểm số (nếu là challenge) lam tron den so thap phan thu 2 ==
    completion_time INT //-- Thời gian hoàn thành tính bằng giây ==
    attempts int //-- Số lần thử ==
    last_attempt_at TIMESTAMP // thoi gian hoan thanh thu thach lan cuoi ==
    completed_at TIMESTAMP ==
    created_at timestamp ==
    updated_at timestamp ==
}
 */