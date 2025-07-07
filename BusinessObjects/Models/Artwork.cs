using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("artworks")]
    public class Artwork
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("id")]
        public long Id { get; set; }

        [Column("image")]
        public string Image { get; set; } = string.Empty;

        [Column("title")]
        public string Title { get; set; } = String.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set;} = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("challenge_id")]
        [ForeignKey("Challenge")]
        public long ChallengeId { get; set; }
        public virtual Challenge Challenge { get; set; } = null!;

        [InverseProperty("Artwork")]
        public virtual ICollection<ArtworkDetail> ArtworkDetails { get; set; } = new List<ArtworkDetail>();
    }
}
