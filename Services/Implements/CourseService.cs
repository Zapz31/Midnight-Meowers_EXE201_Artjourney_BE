using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.Courses;
using Helpers.HelperClasses;
using Microsoft.AspNetCore.Http;
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
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<CourseService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileHandlerService _fileHandlerService;

        public CourseService(ICourseRepository courseRepository, 
            ILogger<CourseService> logger, 
            ICurrentUserService currentUserService,
            IFileHandlerService fileHandlerService)
        {
            _courseRepository = courseRepository;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileHandlerService = fileHandlerService;
        }

        public async Task<ApiResponse<Course>> CreateCourse(CourseDTO courseDTO)
        {
            try
            {
                _logger.LogInformation("Start the CreateCourse function at Courservice");
                var currentUserID = _currentUserService.AccountId;
                var currentUserRole = _currentUserService.Role;
                if (string.IsNullOrEmpty(currentUserRole) || !AccountRole.Admin.ToString().Equals(currentUserRole))
                {
                    ApiResponse<Course> permissionErrorResponse = new()
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Message = "You don't have permission to do this action"
                    };
                    return permissionErrorResponse;
                }

                List<IFormFile> files = new List<IFormFile>();
                string? thumbnailUrl = null;
                if (courseDTO.ThumbnailImage != null)
                {
                    files.Add(courseDTO.ThumbnailImage);
                    var uploadFilesResult = await _fileHandlerService.UploadFiles(files, "a", "a");
                    if (uploadFilesResult.Errors.Any())
                    {
                        ApiResponse<Course> UploadFileErrorResponse = new ApiResponse<Course>()
                        {
                            Status = ResponseStatus.Error,
                            Code = 500,
                            Message = "Server error"
                        };
                    }
                    thumbnailUrl = uploadFilesResult.SuccessfulUploads[0].PresignedUrl;
                }

                Course createCourse = new()
                {
                    Title = courseDTO.Title,
                    ThumbnailUrl = thumbnailUrl,
                    Description = courseDTO.Description,
                    Level = courseDTO.Level,
                    Status = courseDTO.Status,
                    HistoricalPeriodId = courseDTO.HistoricalPeriodId,
                    RegionId = courseDTO.RegionId,
                    Price = 0,
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserID
                };
                var data = await _courseRepository.CreateCourseAsync(createCourse);
                ApiResponse<Course> response = new()
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = data,
                    Message = "Create course success"
                };
                return response;
            } catch (Exception ex)
            {
                _logger.LogError("Error at CreateCourse in CourseService: {ex}", ex.Message);
                ApiResponse<Course> errorResponse = new ApiResponse<Course>()
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "Server error"
                };
                return errorResponse;
            }

            
        }
    }
}
