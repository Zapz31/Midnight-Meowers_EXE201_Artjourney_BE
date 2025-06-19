 using BusinessObjects.Models;
using DAOs;
using Helpers.DTOs.Courses;
using Helpers.DTOs.General;
using Microsoft.EntityFrameworkCore;
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
        private readonly ApplicationDbContext _context;
        public UserRepository(IUnitOfWork unitOfWork, ApplicationDbContext context) 
        {
            _unitOfWork = unitOfWork;
            _context = context;
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

        public async Task CreateAllUserModuleInfo(List<UserModuleInfo> userModuleInfos)
        {
            await _unitOfWork.GetRepo<UserModuleInfo>().CreateAllAsync(userModuleInfos);
        }

        public async Task<List<ModuleSubModuleCourseIds>> GetSingleModuleSubModuleCourseIdsByLCIds(long learningContentId)
        {
            var query = @"select
    sm.sub_module_id as ""SubModuleId"",
    m.module_id as ""ModuleId"",
    m.course_id as ""CourseId""
FROM learning_contents lc
JOIN sub_modules sm ON lc.sub_module_id = sm.sub_module_id
JOIN modules m ON sm.module_id = m.module_id
where
lc.is_active = true and
sm.is_active = true and
m.deleted_at is null and
lc.learning_content_id = {0};";
            var data = await _context.Set<ModuleSubModuleCourseIds>()
                .FromSqlRaw(query, learningContentId)
                .ToListAsync();

            return data;
        }

        public async Task<int> UpdateCourseProgress(long userId, long courseId)
        {
            try
            {
                var completeSQL = string.Format(@"
DO $$
DECLARE
    total_modules INTEGER;
    completed_modules INTEGER;
    total_learning_contents INTEGER;
    completed_learning_contents INTEGER;
    progress_percentage NUMERIC;
    is_course_completed BOOLEAN;
    total_completed_in INTERVAL;
BEGIN
    SELECT COUNT(*) INTO total_modules
    FROM modules 
    WHERE course_id = {1} and deleted_at is null;

    SELECT COUNT(*) INTO completed_modules
    FROM modules m
    JOIN user_module_infos umi ON m.module_id = umi.module_id
    WHERE m.course_id = {1} 
      AND umi.user_id = {0} 
      AND umi.is_completed = true;

    SELECT COUNT(*) INTO total_learning_contents
    FROM learning_contents lc
    JOIN sub_modules sm ON lc.sub_module_id = sm.sub_module_id
    JOIN modules m ON sm.module_id = m.module_id
    WHERE m.course_id = {1} AND lc.is_active = true AND sm.is_active = true AND m.deleted_at is null;

    SELECT COUNT(*) INTO completed_learning_contents
    FROM learning_contents lc
    JOIN sub_modules sm ON lc.sub_module_id = sm.sub_module_id
    JOIN modules m ON sm.module_id = m.module_id
    JOIN user_learning_progresses ulp ON lc.learning_content_id = ulp.learning_content_id
    WHERE m.course_id = {1}
      AND lc.is_active = true
	  AND sm.is_active = true
      AND m.deleted_at is null
      AND ulp.user_id = {0}
      AND ulp.status = 'Completed';

SELECT SUM(umi.completed_in) INTO total_completed_in
    FROM user_module_infos umi
    JOIN modules m ON m.module_id = umi.module_id
    WHERE umi.user_id = {0}
      AND m.course_id = {1}
      AND m.deleted_at is null
      AND umi.is_completed = true;

    progress_percentage := CASE 
        WHEN total_learning_contents = 0 THEN 0
        ELSE ROUND((completed_learning_contents::NUMERIC / total_learning_contents::NUMERIC) * 100)
    END;

    is_course_completed := (total_modules > 0 AND completed_modules = total_modules);

    UPDATE user_course_infos 
    SET 
        learning_status = CASE WHEN is_course_completed THEN 'Completed' ELSE 'InProgress' END,
        progress_percent = progress_percentage,
        completed_at = CASE WHEN is_course_completed AND completed_at IS NULL THEN NOW() ELSE completed_at END,
        completed_in = CASE WHEN is_course_completed AND completed_in IS NULL THEN total_completed_in ELSE completed_in END
    WHERE user_id = {0} AND course_id = {1};
END $$;", userId, courseId);
                var rowEffect = await _context.Database.ExecuteSqlRawAsync(completeSQL, userId, courseId);
                
                return rowEffect;
            } catch (Exception ex)
            {
               
                throw new Exception($"Error when updating progress for course: {ex.Message}", ex);
            }
        }


    }
}
