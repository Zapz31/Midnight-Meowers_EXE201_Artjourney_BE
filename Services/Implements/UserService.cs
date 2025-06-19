using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.Authentication;
using Helpers.DTOs.UserLearningProgress;
using Helpers.DTOs.Users;
using Helpers.HelperClasses;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILoginHistoryRepository _loginHistoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;
        private readonly IUserLearningProgressRepository _userLearningProgressRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILearningContentRepository _learningContentRepository;
        private readonly ISubModuleRepository _subModuleRepository;
        private readonly IModuleRepository _moduleRepository;
        public UserService(
                IUserRepository userRepository,
                ILoginHistoryRepository loginHistoryRepository,
                IUnitOfWork unitOfWork,
                ILogger<UserService> logger,
                IUserLearningProgressRepository userLearningProgressRepository,
                ICurrentUserService currentUserService,
                ILearningContentRepository learningContentRepository,
                ISubModuleRepository subModuleRepository,
                IModuleRepository moduleRepository
            )
        
        {
            _userRepository = userRepository;
            _loginHistoryRepository = loginHistoryRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userLearningProgressRepository = userLearningProgressRepository;
            _currentUserService = currentUserService;
            _learningContentRepository = learningContentRepository;
            _subModuleRepository = subModuleRepository;
            _moduleRepository = moduleRepository;
        }

        public async Task<User> CreateAccount(RegisterDTO registerDTO)
        {
            var user = new User() 
            { 
                Email = registerDTO.Email,
                Password = PasswordHasher.HashPassword(registerDTO.Password),
                Role = registerDTO.Role,
            };
            var createdUser = await _userRepository.CreateUserAsync(user);
            return createdUser;
        }

        public async Task<User> CreateCompleteAccount(User newUser)
        {
            var createdUser = await _userRepository.CreateUserAsync(newUser);
            return createdUser;
        }



        public Task<User?> GetUserByEmailAsync(string email)
        {
            var foundUser = _userRepository.GetUserByEmailAsync(email);
            return foundUser;
        }

        public async Task<ApiResponse<NewUpdateUserDTO?>> GetUserByIDAsynce(long userId)
        {
            try
            {
                var founduser = await _userRepository.GetUserByIDAsync(userId);
                if (founduser == null)
                {
                    return new ApiResponse<NewUpdateUserDTO?>()
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Errors =
                        [
                            new ApiError{Code = 1009}
                        ]
                    };
                }
                NewUpdateUserDTO newUpdateUserDTO = new NewUpdateUserDTO(founduser);
                newUpdateUserDTO.LoginCount = await _loginHistoryRepository.CountLoginHistoriesByUserIdAsync(userId);
                return new ApiResponse<NewUpdateUserDTO?>()
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = newUpdateUserDTO,
                    Message = "2002"
                };
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new ApiResponse<NewUpdateUserDTO?>() 
                { 
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
            
        }

        public async Task<ApiResponse<User>> UpdateUserAsync(NewUpdateUserDTO newUpdateUser)
        {
            try 
            {
                if (newUpdateUser.Email == null)
                {
                    return new ApiResponse<User>
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Errors =
                        [
                            new ApiError { Code = 1002, Message = "Email not found" }
                        ]
                    };
                }

                var updatedUser = await _userRepository.GetUserByEmailAsync(newUpdateUser.Email);
                if (updatedUser == null)
                {
                    return new ApiResponse<User>
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Errors =
                        [
                            new ApiError { Code = 1001, Message = "No user found with this email" }
                        ]
                    };
                }
                bool isUpdate = false;
                // check email
                if (newUpdateUser.Email != null)
                {
                    if (!newUpdateUser.Email.Equals(updatedUser.Email))
                    {
                        updatedUser.Email = newUpdateUser.Email;
                        isUpdate = true;
                    }
                }
                //check fullname
                if (newUpdateUser.FullName != null)
                {
                    if (!newUpdateUser.FullName.Equals(updatedUser.Fullname))
                    {
                        updatedUser.Fullname = newUpdateUser.FullName;
                        isUpdate = true;
                    }
                }
                //check Gender
                if (newUpdateUser.Gender != null)
                {
                    if (!newUpdateUser.Gender.Equals(updatedUser.Gender))
                    {
                        Gender newUpdatedUserGender = (Gender)newUpdateUser.Gender;
                        updatedUser.Gender = newUpdatedUserGender;
                        isUpdate = true;
                    }
                }
                //PhoneNumber
                if (newUpdateUser.PhoneNumber != null)
                {
                    if (!newUpdateUser.PhoneNumber.Equals(updatedUser.PhoneNumber))
                    {
                        updatedUser.PhoneNumber = newUpdateUser.PhoneNumber;
                        isUpdate = true;
                    }
                }
                //Password
                if (newUpdateUser.Password != null)
                {
                    if (!newUpdateUser.Password.Equals(updatedUser.Password))
                    {
                        updatedUser.Password = newUpdateUser.Password;
                        isUpdate = true;
                    }
                }
                //Birthday
                if (newUpdateUser.Birthday != null)
                {
                    if (!newUpdateUser.Birthday.Equals(updatedUser.Birthday))
                    {
                        DateTime newUpdateUserBirthday = (DateTime)newUpdateUser.Birthday;
                        updatedUser.Birthday = newUpdateUserBirthday;
                        isUpdate = true;
                    }
                }
                // check status
                if (newUpdateUser.Status != null)
                {
                    if (!newUpdateUser.Status.Equals(updatedUser.Status))
                    {
                        AccountStatus newUpdateUserStatus = (AccountStatus)newUpdateUser.Status;
                        updatedUser.Status = newUpdateUserStatus;
                        isUpdate = true;
                    }
                }
                //check avatarUrl
                if (newUpdateUser.AvatarUrl != null)
                {
                    if (!newUpdateUser.AvatarUrl.Equals(updatedUser.AvatarUrl))
                    {
                        updatedUser.AvatarUrl = newUpdateUser.AvatarUrl;
                        isUpdate = true;
                    }
                }

                if (isUpdate)
                {
                    updatedUser.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateUserAsync(updatedUser);
                    
                }
                return new ApiResponse<User> 
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = updatedUser
                };
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new ApiResponse<User>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Errors =
                    [
                        new ApiError {Code = 1003}
                    ]
                };
            }
        }

        public async Task<ApiResponse<bool>> LogCourseAccessAsync(UserCourseStreak courseStreak)
        {
            try
            {
                var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);
                _logger.LogInformation("Start LogCourseAccessAsync at UserService.cs");
                await _userRepository.UpdateCourseStreakAsync(courseStreak.UserId, courseStreak.CourseId, todayUtc);
                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = true,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at LogCourseAccessAsync at UserService.cs: {ex}", ex.Message);
                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>> CreateUserLearningProgress (CreateULPRequestDTO requestDTO)
        {
            try
            {
                var userId = _currentUserService.AccountId;

                UserLearningProgress userLearningProgress = new();
                var existULP = await _userLearningProgressRepository.GetUserLearningProgressByUserIdAndLNCId(userId, requestDTO.LearningContentId);
                if (existULP == null)
                {
                    userLearningProgress.Status = requestDTO.Status ?? UserLearningProgressStatus.NotStarted;
                    userLearningProgress.Score = requestDTO.Score ?? 0;
                    userLearningProgress.Attempts = 1;
                    userLearningProgress.LastAttemptAt = DateTime.UtcNow;
                    userLearningProgress.CreatedAt = DateTime.UtcNow;
                    userLearningProgress.LearningContentId = requestDTO.LearningContentId;
                    userLearningProgress.UserId = userId;
                } else
                {
                    userLearningProgress.Status = requestDTO.Status ?? UserLearningProgressStatus.NotStarted;
                    userLearningProgress.Score = requestDTO.Score ?? 0;
                    userLearningProgress.Attempts += 1;
                    userLearningProgress.LastAttemptAt = DateTime.UtcNow;
                    userLearningProgress.LearningContentId = requestDTO.LearningContentId;
                    userLearningProgress.UserId = userId;
                }
                var createdDate = await _userLearningProgressRepository.CreateUserLearningProgressAsync(userLearningProgress);
                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = true,
                };

                //UserLearningProgress userLearningProgress = new() 
                //{
                //    Status = requestDTO.Status ?? UserLearningProgressStatus.NotStarted,
                //    Score = requestDTO.Score ?? 0,
                //    LearningContentId = requestDTO.LearningContentId,

                //};
            } catch (Exception ex)
            {
                _logger.LogError("Error at CreateUserLearningProgress at UserService.cs: {ex}", ex.Message);
                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Data = false,
                    Message = ex.Message             
                };
            }
        }

        public async Task<ApiResponse<bool>> CreateUserLearningProgressesByUserIdAndLNId(long courseId, long userId)
        {
            try
            {
                //var user_id = _currentUserService.AccountId;
                // get all learning_content_id of a course
                var learningContentIdsByCourseIds = await _learningContentRepository.GetLearningContentContentIdsByCourseIdAsync(courseId);
                var existLearningContentIdsByUserId = await _userLearningProgressRepository.GetExistLCIdsByUserId(userId, learningContentIdsByCourseIds, null);
                var newLearningContentIds = learningContentIdsByCourseIds.Except(existLearningContentIdsByUserId).ToList();
                var newProgresses = newLearningContentIds.Select(contentId => new UserLearningProgress
                {
                    UserId = userId,
                    LearningContentId = contentId,
                    Status = UserLearningProgressStatus.InProgress,
                    CreatedAt = DateTime.UtcNow
                }).ToList();
                await _userLearningProgressRepository.CreateAllUserLearningProgressAsync(newProgresses);
                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = true,
                    Message = "Create progresses success"
                };

            } catch (Exception ex)
            {
                _logger.LogError("Error at CreateUserLearningProgressesByUserIdAndLNId at UserService.cs: {ex}", ex.Message);
                return new ApiResponse<bool>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Data = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<UserLearningProgress>> MarkAsCompleteUserLearningProgressSingleAsync(long userLearningProgressId)
        {
            try
            {
                var userId = _currentUserService.AccountId;
                // get updated userLearningProgress
                await _unitOfWork.BeginTransactionAsync();
                var updatedUserLearningProgress = await _userLearningProgressRepository
                    .GetLearningProgressByUserIdAndLNCIdSingle(userId, userLearningProgressId);

                if (updatedUserLearningProgress == null) 
                {
                    return new ApiResponse<UserLearningProgress> 
                    { 
                        Status = ResponseStatus.Error,
                        Code = 400,
                        Message = "UserLearingProgress has not exist!"
                    };
                }

                updatedUserLearningProgress.Status = UserLearningProgressStatus.Completed;
                updatedUserLearningProgress.Attempts += 1;
                if (updatedUserLearningProgress.Attempts < 2)
                {
                    updatedUserLearningProgress.CompletedAt = DateTime.UtcNow;
                    
                }
                updatedUserLearningProgress.CompletedIn = DateTime.UtcNow - updatedUserLearningProgress.StartedAt;
                updatedUserLearningProgress.UpdatedAt = DateTime.UtcNow;

                // update UserLearningProgressSingle
                await _userLearningProgressRepository.UpdateUserLearningProgressSingleAsync(updatedUserLearningProgress);

                // call 3 function here
                var moduleSubModuleCourseIds = await _userRepository.GetSingleModuleSubModuleCourseIdsByLCIds(userLearningProgressId);
                if (!moduleSubModuleCourseIds.Any())
                {
                    return new ApiResponse<UserLearningProgress>
                    {
                        Status = ResponseStatus.Error,
                        Code = 400,
                        Message = "learning content is not valid"
                    };
                }

                await _subModuleRepository.UpdateSubModuleProgress(userId, moduleSubModuleCourseIds[0].SubModuleId ?? 0);
                await _moduleRepository.UpdateModuleProgress(userId, moduleSubModuleCourseIds[0].ModuleId ?? 0);
                await _userRepository.UpdateCourseProgress(userId, moduleSubModuleCourseIds[0].CourseId ?? 0);

                await _unitOfWork.CommitTransactionAsync();
                return new ApiResponse<UserLearningProgress>
                {
                    Status= ResponseStatus.Success,
                    Code = 201,
                    Data = updatedUserLearningProgress,
                    Message = "Update user learning progress success"
                };

            } catch (Exception ex)
            {
                await _unitOfWork.RollBackAsync();
                _logger.LogError("Error at MarkAsCompleteUserLearningProgressSingleAsync at UserService.cs: {ex}", ex.Message);
                return new ApiResponse<UserLearningProgress>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Data = null,
                    Message = ex.Message
                };
            }
        }
    }
}
