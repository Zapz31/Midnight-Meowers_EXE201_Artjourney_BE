using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("challenges")]
    public class Challenge
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("id")]
        public long Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set;} = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get;set;} = DateTime.UtcNow;

        [Column("challenge_type")]
        public string ChallengeType { get; set; } = "DragDrop"; // DragDrop, ...

        [Column("duration_seconds")]
        public long DurationSeconds { get; set; }

        [Column("course_id")]
        [ForeignKey("Course")]
        public long CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;

        [InverseProperty("Challenge")]
        public virtual ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();

        [InverseProperty("Challenge")]
        public virtual ICollection<ChallengeSession> ChallengeSessions { get; set; } = new List<ChallengeSession>();

        [InverseProperty("Challenge")]
        public virtual ICollection<UserChallengeHighestScore> UserChallengeHighestScores { get; set; } = new List<UserChallengeHighestScore>();
    }
}
