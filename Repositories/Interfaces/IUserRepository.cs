using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> CreateUserAsync(User user);
        Task<User?> GetUserByEmailAsync(string email);
        Task UpdateUserAsync(User user);
        Task<User?> GetUserByIDAsync(long userId);
        public Task CreateUserCourseStreak(UserCourseStreak userCourseStreak);
        public Task UpdateUserCourseInfo(UserCourseInfo userCourseInfo);
        public Task UpdateCourseStreakAsync(long userId, long courseId, DateOnly today);
    }
}
