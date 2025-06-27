using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("questions")]
    public class Question
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("question_id")]
        public long QuestionId { get; set; }

        [Column("question_text")]
        public string QuestionText { get; set; } = string.Empty;

        [Column("question_type")]
        public string QuestionType { get; set;} = string.Empty; // SingleChoice or MultipleChoice

        [Column("points", TypeName = "decimal(4,1)")] // point for this question
        public decimal Points { get; set; } = 0;

        [Column("order_index")]
        public int OrderIndex { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("learning_content_id")]
        [ForeignKey("LearningContent")]
        public long LearningContentId { get; set; }
        public virtual LearningContent LearningContent { get; set; } = null!;

        [InverseProperty("Question")]
        public virtual ICollection<QuestionOptions> QuestionOptions { get; set; } = new List<QuestionOptions>();
        
        [InverseProperty("Question")]
        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();

    }
}
