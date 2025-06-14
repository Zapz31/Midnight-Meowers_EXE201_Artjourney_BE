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

        public UserCourseInfoService(IUserCourseInfoRepository userCourseInfoRepository, ILogger<UserCourseInfoService> logger, ICurrentUserService currentUserService)
        {
            _userCourseInfoRepository = userCourseInfoRepository;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<UserCourseInfo>> CreateUserCourseInfo(BasicCreateUserCourseInfoRequestDTO requestDTO)
        {
            try
            {
                UserCourseInfo createUserCourseInfo = new()
                {
                    EnrollmentStatus = requestDTO.EnrollmentStatus,
                    LearningStatus = requestDTO.LearningStatus,
                    UserId = requestDTO.UserId,
                    CourseId = requestDTO.CourseId,
                };
                var responseData = await _userCourseInfoRepository.CreateUserCourseInfo(createUserCourseInfo);
                return new ApiResponse<UserCourseInfo>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = responseData,
                    Message = "Created userCourseInfo success"
                };

            } catch (Exception ex)
            {
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
    }
}
