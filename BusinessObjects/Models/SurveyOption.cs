using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("survey_options")]
    public class SurveyOption
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("survey_option_id")]
        public long SurveyOptionId { get; set; }

        [Column("survey_option_content")]
        public string? SurveyOptionContent { get; set; }

        [Column("is_active")]
        public bool IsAcive { get; set; } = true;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("survey_question_id")]
        [ForeignKey("SurveyQuestion")]
        public long SurveyQuestionId { get; set; }

        // navigation properties
        public virtual SurveyQuestion SurveyQuestion { get; set; } = null!;

        [InverseProperty("SurveyOption")]
        public virtual ICollection<UserOptionChoice> UserOptionChoices { get; set; } = new List<UserOptionChoice>();
    }
}

/*
 table survey_options {
  survey_option_id long [primary key, increment]
  survey_option_content varchar
  is_active bool
  created_at datetime
  //navigate
  survey_question_id long [ref: > survey_questions.survey_question_id]
}
 */
