using BusinessObjects.Models;
using Helpers.DTOs.Authentication;
using Helpers.DTOs.UserLearningProgress;
using Helpers.DTOs.Users;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateAccount(RegisterDTO registerDTO);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateCompleteAccount(User newUser);
        Task<ApiResponse<User>> UpdateUserAsync(NewUpdateUserDTO newUpdateUser);
        Task<ApiResponse<NewUpdateUserDTO?>> GetUserByIDAsynce(long userId);
        public Task<ApiResponse<bool>> LogCourseAccessAsync(UserCourseStreak courseStreak);
        public Task<ApiResponse<bool>> CreateUserLearningProgress(CreateULPRequestDTO requestDTO);
        public Task<ApiResponse<bool>> CreateUserLearningProgressesByUserIdAndLNId(long courseId, long userId);
        public Task<ApiResponse<UserLearningProgress>> MarkAsCompleteUserLearningProgressSingleAsync(long userLearningProgressId);
    }
}
