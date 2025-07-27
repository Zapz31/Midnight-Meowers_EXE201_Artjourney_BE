using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Certificate
{
    public class CertificateDTO
    {
        public long CertificateId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public long CourseId { get; set; }
        public string? CourseName { get; set; }
        public bool IsActive { get; set; }
    }
}
