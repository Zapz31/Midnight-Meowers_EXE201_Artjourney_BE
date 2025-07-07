using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("challenge_sessions")]
    public class ChallengeSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("id")]
        public long Id { get; set; }

        [Column("score")]
        public int Score { get; set; } = 0;

        [Column("time_taken")]
        public long TimeTaken { get; set; } = 0;

        [Column("is_complete")]
        public bool IsComplete { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set;} = DateTime.UtcNow;

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
