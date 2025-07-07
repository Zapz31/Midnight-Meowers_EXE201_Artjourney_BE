using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("artwork_details")]
    public class ArtworkDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("id")]
        public long Id { get; set; }

        [Column("artist")]
        public string Artist { get; set; } = string.Empty;

        [Column("period")]
        public string Period { get; set; } = string.Empty;

        [Column("year")]
        public string Year {  get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set;} = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("artwork_id")]
        [ForeignKey("Artwork")]
        public long ArtworkId { get; set; }
        public virtual Artwork Artwork { get; set; } = null!;


    }
}
