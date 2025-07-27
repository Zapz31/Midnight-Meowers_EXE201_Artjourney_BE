using BusinessObjects.Models;
using Helpers.DTOs.Certificate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ICertificateRepository
    {
        Task<Certificate> CreateCertificateAsync(Certificate certificate);
        Task<Certificate?> GetCertificateByIdAsync(long certificateId);
        Task<Certificate?> GetCertificateByCourseIdAsync(long courseId);
        Task<List<Certificate>> GetAllActiveCertificatesAsync();
        Task<bool> DeleteCertificateAsync(long certificateId);
        Task<bool> UpdateCertificateAsync(Certificate certificate);
    }
}
