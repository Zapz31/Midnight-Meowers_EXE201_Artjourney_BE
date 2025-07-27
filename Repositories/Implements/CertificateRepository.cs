using BusinessObjects.Models;
using DAOs;
using Helpers.DTOs.Certificate;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class CertificateRepository : ICertificateRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public CertificateRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<Certificate> CreateCertificateAsync(Certificate certificate)
        {
            var createdCertificate = await _unitOfWork.GetRepo<Certificate>().CreateAsync(certificate);
            return createdCertificate;
        }

        public async Task<Certificate?> GetCertificateByIdAsync(long certificateId)
        {
            var queryOption = new QueryBuilder<Certificate>()
                .WithTracking(false)
                .WithPredicate(c => c.CertificateId == certificateId && c.IsActive)
                .Build();
            
            var certificates = await _unitOfWork.GetRepo<Certificate>().GetAllAsync(queryOption);
            return certificates.FirstOrDefault();
        }

        public async Task<Certificate?> GetCertificateByCourseIdAsync(long courseId)
        {
            var queryOption = new QueryBuilder<Certificate>()
                .WithTracking(false)
                .WithPredicate(c => c.CourseId == courseId && c.IsActive)
                .Build();
            
            var certificates = await _unitOfWork.GetRepo<Certificate>().GetAllAsync(queryOption);
            return certificates.FirstOrDefault();
        }

        public async Task<List<Certificate>> GetAllActiveCertificatesAsync()
        {
            var queryOption = new QueryBuilder<Certificate>()
                .WithTracking(false)
                .WithPredicate(c => c.IsActive)
                .Build();
            
            var certificates = await _unitOfWork.GetRepo<Certificate>().GetAllAsync(queryOption);
            return certificates.ToList();
        }

        public async Task<bool> DeleteCertificateAsync(long certificateId)
        {
            var certificate = await GetCertificateByIdAsync(certificateId);
            if (certificate == null)
                return false;

            certificate.IsActive = false;
            await _unitOfWork.GetRepo<Certificate>().UpdateAsync(certificate);
            return true;
        }

        public async Task<bool> UpdateCertificateAsync(Certificate certificate)
        {
            await _unitOfWork.GetRepo<Certificate>().UpdateAsync(certificate);
            return true;
        }
    }
}
