using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("user_certificate_infos")]
    public class UserCertificateInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("id")]
        public long Id { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [Column("certificate_id")]
        public long CertificateId { get; set; }

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("completed_in")]
        public TimeSpan? CompletedIn { get; set; }
    }
}
