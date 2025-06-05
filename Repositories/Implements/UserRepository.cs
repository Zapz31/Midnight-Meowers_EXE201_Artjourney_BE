 using BusinessObjects.Models;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserRepository(IUnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<User> CreateUserAsync(User user)
        {
            var createdUser = await _unitOfWork.GetRepo<User>().CreateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return createdUser;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var queryOptions = new QueryBuilder<User>()
                .WithTracking(false)
                .WithPredicate(u => u.Email.Equals(email))
                .Build();
            var foundUser = await _unitOfWork.GetRepo<User>().GetSingleAsync(queryOptions);
            return foundUser;
        }

        public async Task<User?> GetUserByIDAsync(long userId)
        {
            var queryOptions = new QueryBuilder<User>()
                .WithTracking(false)
                .WithPredicate(u => u.UserId == userId)
                .Build();
            var foundUser = await _unitOfWork.GetRepo<User>().GetSingleAsync(queryOptions);
            return foundUser;
        }

        public async Task UpdateUserAsync(User user)
        {
            await _unitOfWork.GetRepo<User>().UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CreateUserCourseStreak (UserCourseStreak userCourseStreak)
        {
            await _unitOfWork.GetRepo<UserCourseStreak>().CreateAsync(userCourseStreak);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateUserCourseInfo(UserCourseInfo userCourseInfo)
        {
            await _unitOfWork.GetRepo<UserCourseInfo>().UpdateAsync(userCourseInfo);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateCourseStreakAsync(long userId, long courseId, DateOnly today)
        {
            var query = new QueryBuilder<UserCourseStreak>()
                .WithTracking(false)
                .WithPredicate(ucs => ucs.UserId == userId && ucs.CourseId == courseId)
                .Build();
            var userCourseStreak = await _unitOfWork.GetRepo<UserCourseStreak>().GetSingleAsync(query);

            if (userCourseStreak == null)
            {
                userCourseStreak = new UserCourseStreak()
                {
                    CourseId = courseId,
                    UserId = userId,
                    CurrentStreak = 1,
                    LongestStreak = 1,
                    TotalDaysAccessed = 1,
                    LastAccessDate = today,
                };
                await _unitOfWork.GetRepo<UserCourseStreak>().CreateAsync(userCourseStreak);
            } else
            {
                if (userCourseStreak.LastAccessDate == today)
                    return; // đã ghi nhận hôm nay rồi

                var yesterday = today.AddDays(-1);
                if (userCourseStreak.LastAccessDate == yesterday)
                {
                    userCourseStreak.CurrentStreak += 1;
                }
                else
                {
                    userCourseStreak.CurrentStreak = 1;
                }

                userCourseStreak.TotalDaysAccessed += 1;
                userCourseStreak.LastAccessDate = today;
                userCourseStreak.LongestStreak = Math.Max(userCourseStreak.LongestStreak, userCourseStreak.CurrentStreak);

                await _unitOfWork.GetRepo<UserCourseStreak>().UpdateAsync(userCourseStreak);
            }
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
