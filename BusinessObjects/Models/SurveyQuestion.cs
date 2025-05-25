using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("survey_questions")]
    public class SurveyQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("survey_question_id")]
        public long SurveyQuestionId { get; set; }

        [Column("survey_question_content")]
        public string? SurveyQuestionName { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("created_by")]
        [ForeignKey("CreatedByUser")]
        public long CreatedBy { get; set; }

        //Navigate properties
        // N - 1
        public virtual User CreatedByUser { get; set; } = null!;

        // 1 - N
        [InverseProperty("SurveyQuestion")]
        public virtual ICollection<SurveyOption> SurveyOptions { get; set; } = new List<SurveyOption>();

    }
}

/*
 table survey_questions {
  survey_question_id long [primary key, increment]
  survey_question_content varchar
  is_active bool
  created_at datetime
  //navigate
  created_by long [ref: > users.id]
}
public virtual ICollection<SurveyQuestion> SurveyQuestions { get; set; } = new List<SurveyQuestion>();
 */
