using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("question_options")]
    public class QuestionOptions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("question_option_id")]
        public long QuestionOptionId { get; set; }

        [Column("option_text")]
        public string OptionText { get; set;} = string.Empty;

        [Column("is_correct")]
        public bool IsCorrect { get; set; } = false;

        [Column("order_index")]
        public int OrderIndex { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set;} = DateTime.UtcNow;

        [Column("question_id")]
        [ForeignKey("Question")]
        public long QuestionId { get; set; }
        public virtual Question Question { get; set; } = null!;

        [InverseProperty("SelectedOption")]
        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();

    }
}
