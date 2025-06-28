using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.UserCourseInfo;
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
    public class UserCourseInfoService : IUserCourseInfoService
    {
        private readonly IUserCourseInfoRepository _userCourseInfoRepository;
        private readonly ILogger<UserCourseInfoService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IModuleRepository _moduleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserSubModuleInfoRepository _userSubModuleInfoRepository;
        private readonly IUserLearningProgressRepository _userLearningProgressRepository;

        public UserCourseInfoService(IUserCourseInfoRepository userCourseInfoRepository, 
            ILogger<UserCourseInfoService> logger, 
            ICurrentUserService currentUserService, 
            IModuleRepository moduleRepository,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            ICourseRepository courseRepository,
            IUserSubModuleInfoRepository userSubModuleInfoRepository,
            IUserLearningProgressRepository userLearningProgressRepository
            )
        {
            _userCourseInfoRepository = userCourseInfoRepository;
            _logger = logger;
            _currentUserService = currentUserService;
            _moduleRepository = moduleRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _userSubModuleInfoRepository = userSubModuleInfoRepository;
            _userLearningProgressRepository = userLearningProgressRepository;
        }

        public async Task<ApiResponse<UserCourseInfo>> CreateUserCourseInfo(BasicCreateUserCourseInfoRequestDTO requestDTO)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                // check if user has enroll this course yet ?
                var userCourseInfo = await _userCourseInfoRepository.GetUserCourseInfosByUserIdAndCourseId(requestDTO.UserId, requestDTO.CourseId);
                if (userCourseInfo.Count > 0)
                {
                    return new ApiResponse<UserCourseInfo>
                    {
                        Status = ResponseStatus.Error,
                        Code = 400,
                        Message = "You have already enrolled this course"
                    };
                }

                UserCourseInfo createUserCourseInfo = new()
                {
                    EnrollmentStatus = requestDTO.EnrollmentStatus,
                    LearningStatus = requestDTO.LearningStatus,
                    UserId = requestDTO.UserId,
                    CourseId = requestDTO.CourseId,
                };

                var modules = await _moduleRepository.GetModulesByCourseIdCompletedAsync(requestDTO.CourseId);
                if (modules.Any())
                {
                    var learningDataIds = await _courseRepository.GetLearningDataIdsByCourseId(requestDTO.CourseId);
                    List<UserModuleInfo> userModuleInfosCreated = new List<UserModuleInfo>();
                    List<UserSubModuleInfo> userSubModuleInfosCreated = new List<UserSubModuleInfo>();
                    List<UserLearningProgress> userLearningProgressesCreated = new List<UserLearningProgress>();
                    if (learningDataIds.ModuleIds.Any())
                    {
                        foreach (var moduleId in learningDataIds.ModuleIds)
                        {
                            var userModuleInfo = new UserModuleInfo
                            {
                                CompletedAt = null,
                                UserId = requestDTO.UserId,
                                ModuleId = moduleId,
                            };
                            userModuleInfosCreated.Add(userModuleInfo);
                        }

                        foreach (var subModuleId in learningDataIds.SubModuleIds)
                        {
                            var userSubModuleInfo = new UserSubModuleInfo
                            {
                                IsCompleted = false,
                                UserId = requestDTO.UserId,
                                SubModuleId = subModuleId,
                            };
                            userSubModuleInfosCreated.Add(userSubModuleInfo);
                        }

                        foreach(var learningContentId in learningDataIds.LearningContentIds)
                        {
                            var userLearningProgress = new UserLearningProgress
                            {
                                UserId = requestDTO.UserId,
                                LearningContentId = learningContentId,
                                Status = UserLearningProgressStatus.InProgress,
                            };
                            userLearningProgressesCreated.Add(userLearningProgress);
                        }

                        // Create
                        await _userRepository.CreateAllUserModuleInfo(userModuleInfosCreated);
                        await _userSubModuleInfoRepository.CreateUserSubModules(userSubModuleInfosCreated);
                        await _userLearningProgressRepository.CreateUserLearningProgressesAsync(userLearningProgressesCreated);
                    }
                }

                var responseData = await _userCourseInfoRepository.CreateUserCourseInfo(createUserCourseInfo);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return new ApiResponse<UserCourseInfo>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = responseData,
                    Message = "Created userCourseInfo success"
                };

            } catch (Exception ex)
            {
                await _unitOfWork.RollBackAsync();
                _logger.LogInformation("Error at CreateUserCourseInfo at UserCourseInfoService: {ex}", ex.Message);
                return new ApiResponse<UserCourseInfo>
                {
                    Status = ResponseStatus.Success,
                    Code = 500,
                    Message = "Error when create UserCoursInfo"
                };
            }
        }

        public async Task<ApiResponse<UserCourseInfo?>> GetUserCourseInfo(long userId, long courseId)
        {
            try
            {
                UserCourseInfo? responseData = null;
                var userCourseReponses = await _userCourseInfoRepository.GetUserCourseInfosByUserIdAndCourseId(userId, courseId);
                if(userCourseReponses.Any())
                {
                    responseData = userCourseReponses[0];
                }
                return new ApiResponse<UserCourseInfo?>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = responseData,
                    Message = "Data retrive success"
                };

            } catch (Exception ex)
            {
                _logger.LogInformation("Error at GetBasicUserCourseInfoGetReponseDTO at UserCourseInfoService: {ex}", ex.Message);
                return new ApiResponse<UserCourseInfo?>
                {
                    Status = ResponseStatus.Success,
                    Code = 500,
                    Message = "Error when create UserCoursInfo"
                };
            }
        }

        public async Task<ApiResponse<List<UserCourseInfo>>> GetUserCourseInfosByUserIdAndCourseIds(long userId, List<long> courseIds)
        {
            try
            {
                var responseData = await _userCourseInfoRepository.GetUserCourseInfosByUserIdAndCourseIds(userId, courseIds);
                return new ApiResponse<List<UserCourseInfo>>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = responseData,
                    Message = "Data retrive success"
                };
            } catch (Exception ex)
            {
                _logger.LogInformation("Error at GetUserCourseInfosByUserIdAndCourseIds at UserCourseInfoService: {ex}", ex.Message);
                return new ApiResponse<List<UserCourseInfo>>
                {
                    Status = ResponseStatus.Success,
                    Code = 500,
                    Message = "Error when create UserCoursInfo"
                };
            }
        }
    }
}
