using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("user_option_choices")]
    public class UserOptionChoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        [Column("id")]
        public long Id { get; set; }

        [Column("content")]
        public string? Content { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get;set; }

        // Navigation properties
        // N - 1
        [Column("user_id")]
        [ForeignKey("User")]
        public long UserId { get; set; }

        public virtual User User { get; set; } = null!;

        [Column("survey_option_id")]
        [ForeignKey("SurveyOption")]
        public long SurveyOptionId { get; set; }

        public virtual SurveyOption SurveyOption { get; set; } = null!;
    }
}

/*
 table user_option_choice {
  id long [primary key, increment]
  user_id long [ref: > users.id]
  survey_option_id long [ref: > survey_options.survey_option_id]
  content text
  created_at datetime
  updated_at datetime
}
 */
