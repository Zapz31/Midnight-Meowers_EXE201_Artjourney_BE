﻿using BusinessObjects.Models;
using DAOs;
using Helpers.DTOs.Courses;
using Helpers.DTOs.UserCourseInfo;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class UserCourseInfoRepository : IUserCourseInfoRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public UserCourseInfoRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<UserCourseInfo> CreateUserCourseInfo(UserCourseInfo userCourseInfo)
        {
                var createdUser = await _unitOfWork.GetRepo<UserCourseInfo>().CreateAsync(userCourseInfo);
                //await _unitOfWork.SaveChangesAsync();
                return createdUser;
        }


        public async Task<List<UserCourseInfo>> GetUserCourseInfosByUserIdAndCourseId(long userId, long courseId)
        {
            var sql = @"select * from user_course_infos uci 
where uci.user_id = {0} and uci.course_id = {1}";
            var data = await _context.Set<UserCourseInfo>()
                .FromSqlRaw(sql, userId, courseId)
                .ToListAsync();
            return data;
        }

        public async Task<List<UserCourseInfo>> GetUserCourseInfosByUserIdAndCourseIds(long userId, List<long> courseId)
        {
            var queryOption = new QueryBuilder<UserCourseInfo>()
                .WithTracking(false)
                .WithPredicate(u => u.UserId == userId && courseId.Contains(u.CourseId))
                .Build();
            var result = await _unitOfWork.GetRepo<UserCourseInfo>().GetAllAsync(queryOption);
            return result.ToList();
        }
    }
}
