using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Certificate
{
    public class CreateCertificateRequestDTO
    {
        [Required]
        public IFormFile CertificateImage { get; set; } = null!;
        
        [Required]
        public long CourseId { get; set; }
    }
}
