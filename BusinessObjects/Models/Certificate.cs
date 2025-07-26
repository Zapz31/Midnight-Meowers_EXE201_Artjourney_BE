using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("certificates")]
    public class Certificate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("certificate_id")]
        public long CertificateId { get; set; }
        
        [Column("image_url")]        
        public string ImageUrl { get; set; } = string.Empty;

        [Column("course_id")]
        public long CourseId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }
    }
}
