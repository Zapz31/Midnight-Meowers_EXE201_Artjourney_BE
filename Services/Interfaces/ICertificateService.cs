using BusinessObjects.Models;
using Helpers.DTOs.Certificate;
using Helpers.HelperClasses;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ICertificateService
    {
        Task<ApiResponse<CertificateDTO>> CreateCertificateAsync(CreateCertificateRequestDTO request);
        Task<ApiResponse<bool>> DeleteCertificateAsync(long certificateId);
        Task<ApiResponse<List<UserCertificateDTO>>> GetUserCertificatesByUserIdAsync(long userId);
        Task<ApiResponse<List<UserCertificateDTO>>> GetUserCertificatesForCurrentUserAsync();
        Task<ApiResponse<List<UserCertificateDTO>>> GetAllUserCertificatesAsync();
        Task<ApiResponse<UserCertificateDTO>> GetCertificateDetailsByIdAsync(long userCertificateId);
        Task<ApiResponse<List<UserCertificateDTO>>> GetUserCertificatesByCourseIdAsync(long courseId);
        Task<ApiResponse<UserCertificateDTO>> AwardCertificateToUserAsync(long userId, long courseId);
    }
}
