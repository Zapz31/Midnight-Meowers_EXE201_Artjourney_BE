using BusinessObjects.Models;
using Helpers.DTOs.Certificate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IUserCertificateInfoRepository
    {
        Task<UserCertificateInfo> CreateUserCertificateAsync(UserCertificateInfo userCertificate);
        Task<List<UserCertificateDTO>> GetUserCertificatesByUserIdAsync(long userId);
        Task<List<UserCertificateDTO>> GetAllUserCertificatesAsync();
        Task<UserCertificateInfo?> GetUserCertificateByUserIdAndCourseIdAsync(long userId, long courseId);
        Task<UserCertificateDTO?> GetCertificateDetailsByIdAsync(long userCertificateId);
        Task<bool> DeleteUserCertificateAsync(long userId, long certificateId);
    }
}
