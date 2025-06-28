using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.Courses;
using Helpers.DTOs.LearningContent;
using Helpers.DTOs.Module;
using Helpers.DTOs.SubModule;
using Helpers.HelperClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
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
        private readonly IModuleRepository _moduleRepository;
        private readonly ISubModuleRepository _subModuleRepository;
        private readonly ILearningContentRepository _learningContentRepository;
        private readonly IUserLearningProgressRepository _userLearningProgressRepository;
        private readonly string ImageBaseUrl = "https://zapzminio.phrimp.io.vn/";

        public CourseService(ICourseRepository courseRepository, 
            ILogger<CourseService> logger, 
            ICurrentUserService currentUserService,
            IFileHandlerService fileHandlerService,
            IModuleRepository moduleRepository,
            ISubModuleRepository subModuleRepository,
            ILearningContentRepository learningContentRepository,
            IUserLearningProgressRepository userLearningProgressRepository
            )
        {
            _courseRepository = courseRepository;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileHandlerService = fileHandlerService;
            _moduleRepository = moduleRepository;
            _subModuleRepository = subModuleRepository;
            _learningContentRepository = learningContentRepository;
            _userLearningProgressRepository = userLearningProgressRepository;
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
                // thumbnail upload
                List<IFormFile> thumbnailfiles = new List<IFormFile>();
                string? thumbnailUrl = null;
                string? realThumbnailUrl = null;
                if (courseDTO.ThumbnailImage != null)
                {
                    thumbnailfiles.Add(courseDTO.ThumbnailImage);
                    var uploadFilesResult = await _fileHandlerService.UploadFiles(thumbnailfiles, "a", "ThumbnailImage");
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
                if(!string.IsNullOrEmpty(thumbnailUrl))
                {
                    Uri uri = new Uri(thumbnailUrl);
                    string pathAndFileName = uri.PathAndQuery.TrimStart('/');
                    realThumbnailUrl = ImageBaseUrl + pathAndFileName;
                }
                // coverimage upload
                List<IFormFile> coverImagefiles = new List<IFormFile>();
                string? coverImageUrl = null;
                string? realCoverImageUrl = null;
                if (courseDTO.CoverImage != null)
                {
                    coverImagefiles.Add(courseDTO.CoverImage);
                    var uploadFilesResult = await _fileHandlerService.UploadFiles(coverImagefiles, "a", "CoverImage");
                    if (uploadFilesResult.Errors.Any())
                    {
                        ApiResponse<Course> UploadFileErrorResponse = new ApiResponse<Course>()
                        {
                            Status = ResponseStatus.Error,
                            Code = 500,
                            Message = "Server error"
                        };
                    }
                    coverImageUrl = uploadFilesResult.SuccessfulUploads[0].PresignedUrl;
                }
                if (coverImageUrl != null)
                {
                    Uri uri = new Uri(coverImageUrl);
                    string pathAndFileName = uri.PathAndQuery.TrimStart('/'); // Loại bỏ dấu / đầu tiên

                    realCoverImageUrl = ImageBaseUrl + pathAndFileName;
                }
                



                Course createCourse = new()
                {
                    Title = courseDTO.Title,
                    ThumbnailUrl = realThumbnailUrl,
                    CoverImageUrl = realCoverImageUrl,
                    Description = courseDTO.Description,
                    LearningOutcomes = courseDTO.LearningOutcomes,
                    EstimatedDuration = courseDTO.EstimatedDuration,
                    Level = courseDTO.Level,
                    Status = courseDTO.Status,
                    HistoricalPeriodId = courseDTO.HistoricalPeriodId,
                    RegionId = courseDTO.RegionId,
                    Price = 0,
                    IsFeatured = false,
                    IsPremium = courseDTO.IsPremium ?? false,
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

        public async Task<ApiResponse<List<LearnPageCourseReginDTO>>> GetAllPublishedCoursesGroupedByRegionAsync()
        {
            try
            {
                _logger.LogInformation("Start GetAllPublishedCoursesAsync in CourseService.cs");
                var data = await _courseRepository.GetAllPublishedCoursesGroupedByRegionAsync();
                ApiResponse<List<LearnPageCourseReginDTO>> response = new()
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = data,
                    Message = "Get all course success"
                };
                return response;
            } catch (Exception ex)
            {
                _logger.LogError("Error at GetAllPublishedCoursesAsync in CourseService.cs: {ex}", ex.Message);
                ApiResponse<List<LearnPageCourseReginDTO>> errorResponse = new ApiResponse<List<LearnPageCourseReginDTO>>()
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = "Server error"
                };
                return errorResponse;
            }
        }

        public async Task<ApiResponse<PaginatedResult<SearchResultCourseDTO>>> SearchCoursesAsync(string? input, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Start SearchCoursesAsync at CourseService.cs ");
                var data = await _courseRepository.SearchCoursesAsync(input, pageNumber, pageSize);
                return new ApiResponse<PaginatedResult<SearchResultCourseDTO>>()
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = data,
                    Message = "Retrive course success"
                };
            }catch (Exception ex)
            {
                _logger.LogError("Error at SearchCoursesAsync at CourseService.cs: {ex}", ex.Message);
                return new ApiResponse<PaginatedResult<SearchResultCourseDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<CourseDetailResponseDTO>> GetCourseDetailAsync(long courseId)
        {
            try
            {
                var userId = _currentUserService.AccountId;
                var course = await _courseRepository.GetSingleCourseAsync(courseId);
                if (course == null)
                {
                    return new ApiResponse<CourseDetailResponseDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 400,
                        Message = $"Cannot fine course with id:{courseId}"
                    };
                }
                CourseDetailResponseDTO responseData = new()
                {
                    CourseId = course.CourseId,
                    Title = course.Title,
                    Description = course.Description,
                    CoverImageUrl = course.CoverImageUrl,
                    CourseLevel = course.Level,
                    LearningOutcomes = course.LearningOutcomes,
                    IsPremium = course.IsPremium,
                    Price = course.Price,
                };

                // Get total time limit and total quantity of learning content of a course
                var totalCourseStatic = await _courseRepository.GetCourseLearningStatisticsOptimizedAsync(courseId);

                List<ModuleCourseDetailScreenResponseDTO> moduleResponseDTOs = new List<ModuleCourseDetailScreenResponseDTO>();

                // Get modules
                var modules = await _moduleRepository.GetModulesByCourseId(courseId);
                if (!modules.Any()) 
                {

                    return new ApiResponse<CourseDetailResponseDTO>
                    {
                        Status = ResponseStatus.Success,
                        Code = 200,
                        Data = responseData,
                        Message = "Course detail retrive success"
                    };
                }

                var moduleIds = modules.Select(m => m.ModuleId).ToList();

                // Get submodules
                var subModules = await _subModuleRepository.GetSubModulesByModuleIds(moduleIds);
                var subModuleIds = subModules.Select(sm => sm.SubModuleId).ToList();

                // Get learning content
                var learningContents = await _learningContentRepository.GetLearningContentsBySubmoduleIds(subModuleIds);
                var learningContentIds = learningContents.Select(lc => lc.LearningContentId).ToList();

                // Get user learning progress
                var userProgresses = await _userLearningProgressRepository.GetLearningProgressByUserIdAndLNCIds(userId, learningContentIds);

                var progressLookup = userProgresses.ToLookup(p => p.LearningContentId);
                var contentsBySubModule = learningContents.ToLookup(lc => lc.SubModuleId);
                var subModulesByModule = subModules.ToLookup(sm => sm.ModuleId);

                moduleResponseDTOs = modules.Select(module => new ModuleCourseDetailScreenResponseDTO
                {
                    ModuleId = module.ModuleId,
                    ModuleTitle = module.ModuleTitle,
                    subModuleCourseDetailScreenResponseDTOs = subModulesByModule[module.ModuleId]
                        .OrderBy(sm => sm.DisplayOrder)
                        .Select(subModule => new SubModuleCourseDetailScreenResponseDTO
                            {
                                SubModuleId = subModule.ModuleId,
                                SubModuleTitle = subModule.SubModuleTitle,
                                learningContentDetailScreenResponseDTOs = contentsBySubModule[subModule.SubModuleId]
                                    .OrderBy(lc => lc.DisplayOrder)
                                    .Select(learningContent => new LearningContentDetailScreenResponseDTO
                                    {
                                        LearningContentId = learningContent.LearningContentId,
                                        LearningContentTitle = learningContent.Title,
                                        userLearningProgressStatus = progressLookup[learningContent.LearningContentId]
                                            .FirstOrDefault()?.Status ?? UserLearningProgressStatus.NotStarted
                                    }).ToList()
                            }).ToList()
                }).ToList();

                var totalLearningContents = learningContents.Count();
                var completedLearningContents = userProgresses.Count(ulp => ulp.Status == UserLearningProgressStatus.Completed);

                var courseCompletionPercentage = totalLearningContents > 0
                    ? (int)Math.Round((double)completedLearningContents / totalLearningContents * 100)
                    : 0;

                // Tính thời gian dựa trên time_limit
                var totalTimeMinutes = learningContents
                    .Where(lc => lc.TimeLimit.HasValue)
                    .Sum(lc => lc.TimeLimit.Value.TotalMinutes);

                var completedTimeMinutes = learningContents
                    .Where(lc => lc.TimeLimit.HasValue &&
                                progressLookup[lc.LearningContentId].Any(p => p.Status == UserLearningProgressStatus.Completed))
                    .Sum(lc => lc.TimeLimit.Value.TotalMinutes);

                var remainingTimeMinutes = Math.Max(0, totalTimeMinutes - completedTimeMinutes);
                var timeSpentPercentage = totalTimeMinutes > 0
                    ? (int)Math.Round(completedTimeMinutes / totalTimeMinutes * 100)
                    : 0;

                responseData.TotalModule = modules.Count();
                responseData.ModuleCourseDetailScreenResponseDTOs = moduleResponseDTOs;
                responseData.CourseCompletionPercentage = courseCompletionPercentage;
                responseData.TimeSpentPercentage = timeSpentPercentage;
                responseData.RemainingTime = TimeSpan.FromMinutes(remainingTimeMinutes);
                return new ApiResponse<CourseDetailResponseDTO>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = responseData,
                    Message = "Course Detail retrive success"

                };
            } catch (Exception ex)
            {
                _logger.LogError("Error at GetCourseDetailAsync at CourseService.cs: {ex}", ex.Message);
                return new ApiResponse<CourseDetailResponseDTO>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<CourseDetailResponseDTO>> GetCourseDetailForGuestAsync(long courseId)
        {
            try
            {
                // get course
                var course = await _courseRepository.GetSingleCourseAsync(courseId);
                if (course == null)
                {
                    return new ApiResponse<CourseDetailResponseDTO>
                    {
                        Status = ResponseStatus.Error,
                        Code = 400,
                        Message = $"Cannot fine course with id:{courseId}"
                    };
                }
                CourseDetailResponseDTO responseData = new()
                {
                    CourseId = course.CourseId,
                    Title = course.Title,
                    Description = course.Description,
                    CoverImageUrl = course.CoverImageUrl,
                    CourseLevel = course.Level,
                    LearningOutcomes = course.LearningOutcomes,
                    IsPremium = course.IsPremium,
                    Price = course.Price,
                };
                // get course detail flat
                var courseDetailFlats = await _courseRepository.GetCourseDetailScreenFlatAsync(courseId);

                var moduleGroups = courseDetailFlats
    .GroupBy(x => new { x.ModuleId, x.ModuleTitle })
    .OrderBy(g => g.Key.ModuleId);

                foreach (var moduleGroup in moduleGroups)
                {
                    var moduleDto = new ModuleCourseDetailScreenResponseDTO
                    {
                        ModuleId = moduleGroup.Key.ModuleId,
                        ModuleTitle = moduleGroup.Key.ModuleTitle,
                        subModuleCourseDetailScreenResponseDTOs = new List<SubModuleCourseDetailScreenResponseDTO>()
                    };

                    // Group by SubModule within each Module
                    var subModuleGroups = moduleGroup
                        .GroupBy(x => new { x.SubModuleId, x.SubModuleTitle })
                        .OrderBy(g => g.Key.SubModuleId);

                    foreach (var subModuleGroup in subModuleGroups)
                    {
                        var subModuleDto = new SubModuleCourseDetailScreenResponseDTO
                        {
                            SubModuleId = subModuleGroup.Key.SubModuleId, // Thêm dòng này
                            SubModuleTitle = subModuleGroup.Key.SubModuleTitle,
                            learningContentDetailScreenResponseDTOs = new List<LearningContentDetailScreenResponseDTO>() // Thêm dòng này
                        };

                        // Group và mapping Learning Contents cho mỗi SubModule
                        var learningContents = subModuleGroup
                            .OrderBy(x => x.DisplayOrder)
                            .ThenBy(x => x.LearningContentId);

                        foreach (var learningContent in learningContents)
                        {
                            var learningContentDto = new LearningContentDetailScreenResponseDTO
                            {
                                LearningContentId = learningContent.LearningContentId,
                                LearningContentTitle = learningContent.Title
                                // userLearningProgressStatus sẽ được set ở nơi khác hoặc để null như yêu cầu
                            };

                            subModuleDto.learningContentDetailScreenResponseDTOs.Add(learningContentDto);
                        }

                        moduleDto.subModuleCourseDetailScreenResponseDTOs.Add(subModuleDto);
                    }

                    responseData.ModuleCourseDetailScreenResponseDTOs.Add(moduleDto);
                }

                responseData.TotalModule = responseData.ModuleCourseDetailScreenResponseDTOs.Count;
                return new ApiResponse<CourseDetailResponseDTO>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = responseData,
                    Message = "Data retrive success"
                };

            }
            catch (Exception ex)
            {
                _logger.LogError("Error at GetCourseDetailForGuestAsync at CourseService.cs: {ex}", ex.Message);
                return new ApiResponse<CourseDetailResponseDTO>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<CourseHasEnrolledBasicViewReponseDTO>>> GetCoursesHasEnrolledByUserIdAsync(long userId)
        {
            try
            {
                var courses = await _courseRepository.GetQueryResultBFlat(userId);
                if (courses.Count == 0)
                {
                    return new ApiResponse<List<CourseHasEnrolledBasicViewReponseDTO>>
                    {
                        Status = ResponseStatus.Error,
                        Code = 400,
                        Message = "User has not enrolled any course"
                    };
                }
                var courseIds = courses.Select(c => c.CourseId).ToList();
                var modules = await _courseRepository.GetModuleCourseHasEnrolledBasicViewDTsAsync(userId, courseIds);
                var moduleIds = modules.Select(m => m.ModuleId).ToList();
                var subModules = await _courseRepository.GetSubModuleCourseHasEnrolledBasicViewDTOsAsync(userId, moduleIds);

                var moduleGroups = modules.GroupBy(m => m.CourseId).ToDictionary(g => g.Key, g => g.ToList());
                var subModuleGroups = subModules.GroupBy(sm => sm.ModuleId).ToDictionary(g => g.Key, g => g.ToList());
                var optimizedResult = courses.Select(course => new CourseHasEnrolledBasicViewReponseDTO
                {
                    CourseId = course.CourseId,
                    CourseTitle = course.CourseTitle,
                    CourseDescription = course.CourseDescription,
                    ThumbnailUrl = course.ThumbnailUrl,
                    RegionName = course.RegionName,
                    HistorialPeriodName = course.HistorialPeriodName,
                    CompletedAt = course.CompletedAt,
                    Modules = moduleGroups.ContainsKey(course.CourseId)
                        ? moduleGroups[course.CourseId].Select(module => new ModuleCourseHasEnrolledBasicViewDTO
                        {
                            ModuleId = module.ModuleId,
                            IsCompleted = module.IsCompleted,
                            CourseId = module.CourseId,
                            SubModules = subModuleGroups.ContainsKey(module.ModuleId)
                                ? subModuleGroups[module.ModuleId].Select(subModule => new SubModuleCourseHasEnrolledBasicViewDTO
                                {
                                    SubModuleId = subModule.SubModuleId,
                                    IsCompleted = subModule.IsCompleted,
                                    ModuleId = subModule.ModuleId
                                }).ToList()
                                : new List<SubModuleCourseHasEnrolledBasicViewDTO>()
                        }).ToList()
                    : new List<ModuleCourseHasEnrolledBasicViewDTO>()
                }).ToList();

                return new ApiResponse<List<CourseHasEnrolledBasicViewReponseDTO>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = optimizedResult,
                    Message = "Data retrieve success"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at GetCoursesHasEnrolledByUserIdAsync at CourseService.cs: {ex}", ex.Message);
                return new ApiResponse<List<CourseHasEnrolledBasicViewReponseDTO>>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }
    }
}
