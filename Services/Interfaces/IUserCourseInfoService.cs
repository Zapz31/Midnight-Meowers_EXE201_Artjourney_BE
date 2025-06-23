using BusinessObjects.Models;
using Helpers.DTOs.UserCourseInfo;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IUserCourseInfoService
    {
        public Task<ApiResponse<UserCourseInfo>> CreateUserCourseInfo(BasicCreateUserCourseInfoRequestDTO requestDTO);
        public Task<ApiResponse<UserCourseInfo?>> GetUserCourseInfo(long userId, long courseId);
        public Task<ApiResponse<List<UserCourseInfo>>> GetUserCourseInfosByUserIdAndCourseIds(long userId, List<long> courseIds);
    }
}
