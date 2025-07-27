using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.Certificate;
using Helpers.HelperClasses;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implements
{
    public class CertificateService : ICertificateService
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly IUserCertificateInfoRepository _userCertificateInfoRepository;
        private readonly IUserCourseInfoRepository _userCourseInfoRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IFileHandlerService _fileHandlerService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CertificateService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly string ImageBaseUrl = "https://zapzminio.phrimp.io.vn/";

        public CertificateService(
            ICertificateRepository certificateRepository,
            IUserCertificateInfoRepository userCertificateInfoRepository,
            IUserCourseInfoRepository userCourseInfoRepository,
            ICourseRepository courseRepository,
            IFileHandlerService fileHandlerService,
            IUnitOfWork unitOfWork,
            ILogger<CertificateService> logger,
            ICurrentUserService currentUserService)
        {
            _certificateRepository = certificateRepository;
            _userCertificateInfoRepository = userCertificateInfoRepository;
            _userCourseInfoRepository = userCourseInfoRepository;
            _courseRepository = courseRepository;
            _fileHandlerService = fileHandlerService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<CertificateDTO>> CreateCertificateAsync(CreateCertificateRequestDTO request)
        {
            try
            {
                var currentUserRole = _currentUserService.Role;
                if (string.IsNullOrEmpty(currentUserRole) || !AccountRole.Admin.ToString().Equals(currentUserRole))
                {
                    return new ApiResponse<CertificateDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Message = "You don't have permission to create certificates"
                    };
                }

                // Check if course exists
                var course = await _courseRepository.GetSingleCourseAsync(request.CourseId);
                if (course == null)
                {
                    return new ApiResponse<CertificateDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Message = "Course not found"
                    };
                }

                // Check if certificate already exists for this course
                var existingCertificate = await _certificateRepository.GetCertificateByCourseIdAsync(request.CourseId);
                if (existingCertificate != null)
                {
                    return new ApiResponse<CertificateDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 400,
                        Message = "Certificate already exists for this course"
                    };
                }

                await _unitOfWork.BeginTransactionAsync();

                // Upload certificate image
                var uploadFiles = new List<Microsoft.AspNetCore.Http.IFormFile> { request.CertificateImage };
                var uploadResult = await _fileHandlerService.UploadFiles(uploadFiles, course.Title, "CertificateTemplate");

                if (uploadResult.Errors.Any())
                {
                    await _unitOfWork.RollBackAsync();
                    return new ApiResponse<CertificateDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 500,
                        Message = "Failed to upload certificate image: " + string.Join(", ", uploadResult.Errors)
                    };
                }

                var imageUrl = uploadResult.SuccessfulUploads.First().PresignedUrl;
                string? realImageUrl = null;
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    var uri = new Uri(imageUrl);
                    string pathAndFileName = uri.PathAndQuery.TrimStart('/');
                    realImageUrl = ImageBaseUrl + pathAndFileName;
                }

                // Create certificate
                var certificate = new Certificate
                {
                    ImageUrl = realImageUrl ?? "",
                    CourseId = request.CourseId,
                    IsActive = true
                };

                var createdCertificate = await _certificateRepository.CreateCertificateAsync(certificate);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Certificate created successfully for course {CourseId} by user {UserId}", 
                    request.CourseId, _currentUserService.AccountId);

                return new ApiResponse<CertificateDTO>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = new CertificateDTO
                    {
                        CertificateId = createdCertificate.CertificateId,
                        ImageUrl = createdCertificate.ImageUrl,
                        CourseId = createdCertificate.CourseId,
                        CourseName = course.Title,
                        IsActive = createdCertificate.IsActive
                    },
                    Message = "Certificate created successfully"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollBackAsync();
                _logger.LogError("Error creating certificate for course {CourseId}: {Error}", request.CourseId, ex.Message);
                return new ApiResponse<CertificateDTO>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "An error occurred while creating the certificate"
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteCertificateAsync(long certificateId)
        {
            try
            {
                var currentUserRole = _currentUserService.Role;
                if (string.IsNullOrEmpty(currentUserRole) || !AccountRole.Admin.ToString().Equals(currentUserRole))
                {
                    return new ApiResponse<bool>
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Message = "You don't have permission to delete certificates"
                    };
                }

                await _unitOfWork.BeginTransactionAsync();

                var result = await _certificateRepository.DeleteCertificateAsync(certificateId);
                if (!result)
                {
                    await _unitOfWork.RollBackAsync();
                    return new ApiResponse<bool>
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Message = "Certificate not found"
                    };
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Certificate {CertificateId} deleted by user {UserId}", 
                    certificateId, _currentUserService.AccountId);

                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = true,
                    Message = "Certificate deleted successfully"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollBackAsync();
                _logger.LogError("Error deleting certificate {CertificateId}: {Error}", certificateId, ex.Message);
                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "An error occurred while deleting the certificate"
                };
            }
        }

        public async Task<ApiResponse<List<UserCertificateDTO>>> GetUserCertificatesByUserIdAsync(long userId)
        {
            try
            {
                var certificates = await _userCertificateInfoRepository.GetUserCertificatesByUserIdAsync(userId);

                return new ApiResponse<List<UserCertificateDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = certificates,
                    Message = certificates.Any() ? "Certificates retrieved successfully" : "No certificates found for this user"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving certificates for user {UserId}: {Error}", userId, ex.Message);
                return new ApiResponse<List<UserCertificateDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "An error occurred while retrieving certificates"
                };
            }
        }

        public async Task<ApiResponse<List<UserCertificateDTO>>> GetUserCertificatesForCurrentUserAsync()
        {
            try
            {
                var currentUserId = _currentUserService.AccountId;
                if (currentUserId <= 0)
                {
                    return new ApiResponse<List<UserCertificateDTO>>
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Message = "User not authenticated"
                    };
                }

                var certificates = await _userCertificateInfoRepository.GetUserCertificatesByUserIdAsync(currentUserId);

                return new ApiResponse<List<UserCertificateDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = certificates,
                    Message = certificates.Any() ? "Certificates retrieved successfully" : "No certificates found for this user"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving certificates for current user: {Error}", ex.Message);
                return new ApiResponse<List<UserCertificateDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "An error occurred while retrieving certificates"
                };
            }
        }

        public async Task<ApiResponse<List<UserCertificateDTO>>> GetAllUserCertificatesAsync()
        {
            try
            {
                var currentUserRole = _currentUserService.Role;
                if (string.IsNullOrEmpty(currentUserRole) || !AccountRole.Admin.ToString().Equals(currentUserRole))
                {
                    return new ApiResponse<List<UserCertificateDTO>>
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Message = "You don't have permission to view all certificates"
                    };
                }

                var certificates = await _userCertificateInfoRepository.GetAllUserCertificatesAsync();

                return new ApiResponse<List<UserCertificateDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = certificates,
                    Message = certificates.Any() ? "All certificates retrieved successfully" : "No certificates found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving all certificates: {Error}", ex.Message);
                return new ApiResponse<List<UserCertificateDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "An error occurred while retrieving certificates"
                };
            }
        }

        public async Task<ApiResponse<UserCertificateDTO>> GetCertificateDetailsByIdAsync(long userCertificateId)
        {
            try
            {
                var certificate = await _userCertificateInfoRepository.GetCertificateDetailsByIdAsync(userCertificateId);

                if (certificate == null)
                {
                    return new ApiResponse<UserCertificateDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Message = "Certificate not found"
                    };
                }

                // Check if the current user has permission to view this certificate
                var currentUserId = _currentUserService.AccountId;
                var currentUserRole = _currentUserService.Role;
                
                // Only allow admin or the certificate owner to view the details
                if (!AccountRole.Admin.ToString().Equals(currentUserRole) && currentUserId != certificate.UserId)
                {
                    return new ApiResponse<UserCertificateDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 403,
                        Message = "You don't have permission to view this certificate"
                    };
                }

                return new ApiResponse<UserCertificateDTO>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = certificate,
                    Message = "Certificate details retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving certificate details for ID {UserCertificateId}: {Error}", userCertificateId, ex.Message);
                return new ApiResponse<UserCertificateDTO>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "An error occurred while retrieving certificate details"
                };
            }
        }

        public async Task<ApiResponse<UserCertificateDTO>> AwardCertificateToUserAsync(long userId, long courseId)
        {
            try
            {
                // Check if user has completed the course
                var userCourseInfos = await _userCourseInfoRepository.GetUserCourseInfosByUserIdAndCourseId(userId, courseId);
                var userCourseInfo = userCourseInfos.FirstOrDefault();

                if (userCourseInfo == null)
                {
                    return new ApiResponse<UserCertificateDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Message = "User is not enrolled in this course"
                    };
                }

                if (userCourseInfo.LearningStatus != CourseLearningStatus.Completed)
                {
                    return new ApiResponse<UserCertificateDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 400,
                        Message = "User has not completed this course yet"
                    };
                }

                // Check if certificate exists for this course
                var certificate = await _certificateRepository.GetCertificateByCourseIdAsync(courseId);
                if (certificate == null)
                {
                    return new ApiResponse<UserCertificateDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Message = "No certificate template found for this course"
                    };
                }

                // Check if user already has this certificate
                var existingUserCertificate = await _userCertificateInfoRepository.GetUserCertificateByUserIdAndCourseIdAsync(userId, courseId);
                if (existingUserCertificate != null)
                {
                    return new ApiResponse<UserCertificateDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 400,
                        Message = "User already has a certificate for this course"
                    };
                }

                await _unitOfWork.BeginTransactionAsync();

                // Award certificate to user
                var userCertificate = new UserCertificateInfo
                {
                    UserId = userId,
                    CertificateId = certificate.CertificateId,
                    CompletedAt = userCourseInfo.CompletedAt,
                    CompletedIn = userCourseInfo.CompletedIn
                };

                var createdUserCertificate = await _userCertificateInfoRepository.CreateUserCertificateAsync(userCertificate);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Get course info for response
                var course = await _courseRepository.GetSingleCourseAsync(courseId);

                _logger.LogInformation("Certificate awarded to user {UserId} for course {CourseId}", userId, courseId);

                return new ApiResponse<UserCertificateDTO>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = new UserCertificateDTO
                    {
                        Id = createdUserCertificate.Id,
                        UserId = createdUserCertificate.UserId,
                        CertificateId = createdUserCertificate.CertificateId,
                        CertificateImageUrl = certificate.ImageUrl,
                        CourseId = courseId,
                        CourseName = course?.Title ?? "",
                        CompletedAt = createdUserCertificate.CompletedAt,
                        CompletedIn = createdUserCertificate.CompletedIn
                    },
                    Message = "Certificate awarded successfully"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollBackAsync();
                _logger.LogError("Error awarding certificate to user {UserId} for course {CourseId}: {Error}", 
                    userId, courseId, ex.Message);
                return new ApiResponse<UserCertificateDTO>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "An error occurred while awarding the certificate"
                };
            }
        }
    }
}
