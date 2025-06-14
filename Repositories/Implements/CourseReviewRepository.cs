using BusinessObjects.Models;
using DAOs;
using Helpers.DTOs.CourseReivew;
using Helpers.DTOs.Courses;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class CourseReviewRepository : ICourseReviewRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICourseRepository _courseRepository;
        private readonly ApplicationDbContext _context;
        public CourseReviewRepository(IUnitOfWork unitOfWork, ICourseRepository courseRepository, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _courseRepository = courseRepository;
            _context = context;
        }

        public async Task<CourseReview> CreateCourseReview(CourseReview courseReview)
        {
                var createdCourseReview = await _unitOfWork.GetRepo<CourseReview>().CreateAsync(courseReview);
                await _unitOfWork.SaveChangesAsync();
                return createdCourseReview;
        }

        public async Task<List<BasicCourseReviewFlatResponseDTO>> GetBasicCourseReviewFlatResponseDTOsByCourseId(long courseId)
        {
            var sql = @"select 
	                u.fullname as ""FullName"",
	                u.avatar_url as ""AvatarUrl"",
	                cr.course_review_id as ""CourseReviewId"",
	                cr.user_id as ""UserId"",
	                cr.course_id as ""CourseId"",
	                cr.rating as ""Rating"",
	                cr.feedback as ""Feedback"",
	                cr.created_at as ""CreatedAt"",
	                cr.updated_at as ""UpdatedAt"",
	                cr.is_approved as ""IsApproved""
                from course_reviews cr 
                inner join users u on u.user_id = cr.user_id
                where cr.is_approved = true and cr.course_id = {0}";

            var data = await _context.Set<BasicCourseReviewFlatResponseDTO>()
                .FromSqlRaw(sql, courseId)
                .ToListAsync();
            return data;
        }

    }
}
