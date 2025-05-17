using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("verification_info")]
    public class VerificationInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("token")]
        public string? Token { get; set; }

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

    }
}
