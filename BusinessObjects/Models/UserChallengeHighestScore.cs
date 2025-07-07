using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("user_challenge_highest_scores")]
    public class UserChallengeHighestScore
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("id")]
        public long Id { get; set; }

        [Column("highest_score")]
        public long HighestScore { get; set; }

        [Column("time_taken")]
        public long TimeTaken { get; set; } = 0;

        [Column("attempted_at")]
        public DateTime AttemptedAt = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt {  get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public long UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Column("challenge_id")]
        [ForeignKey("Challenge")]
        public long ChallengeId { get; set; }
        public virtual Challenge Challenge { get; set; } = null!;
    }
}
