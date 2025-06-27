using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("user_answers")]
    public class UserAnswer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("id")]
        public long Id { get; set; }

        [Column("quiz_attempt_id")]
        [ForeignKey("QuizAttempt")]
        public long QuizAttemptId { get; set; }
        public virtual QuizAttempt QuizAttempt { get; set; } = null!;

        [Column("question_id")]
        [ForeignKey("Question")]
        public long QuestionId { get; set; }
        public virtual Question Question { get; set; } = null!;

        [Column("selected_option_id")]
        [ForeignKey("SelectedOption")]
        public long SelectedOptionId { get; set; }
        public virtual QuestionOptions SelectedOption { get; set; } = null!;

        [Column("answered_at")]
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    }
}
