using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Certificate
{
    public class UserCertificateDTO
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long CertificateId { get; set; }
        public string CertificateImageUrl { get; set; } = string.Empty;
        public long CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
        public TimeSpan? CompletedIn { get; set; }
    }
}
