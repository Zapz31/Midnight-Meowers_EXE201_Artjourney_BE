using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.Courses;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class CourseRepository : ICourseRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        public CourseRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            var createdCourse = await _unitOfWork.GetRepo<Course>().CreateAsync(course);
            await _unitOfWork.SaveChangesAsync();
            return createdCourse;
        }

        public async Task<List<CourseDTO>> GetAllPublishedCoursesAsync()
        {
           
            var queryOptions = new QueryBuilder<Course>()
                .WithPredicate(c => c.Status == CourseStatus.Published)
                .WithInclude(
                    c => c.CourseHistoricalPeriod,
                    c => c.CourseRegion
                )
                .WithTracking(false)
                .Build();

            var publishedCourses = await _unitOfWork.GetRepo<Course>().GetAllAsync(queryOptions);

            // Map từ Course sang CourseDTO
            var courseDTOs = publishedCourses.Select(course => new CourseDTO
            {
                CourseId = course.CourseId,
                Title = course.Title,
                ThumbnailImageUrl = course.ThumbnailUrl,
                Description = course.Description,
                Level = course.Level,
                Status = course.Status,
                HistoricalPeriodId = course.HistoricalPeriodId,
                HistoricalPeriodName = course.CourseHistoricalPeriod?.HistoricalPeriodName, // Giả sử HistoricalPeriod có property Name
                RegionId = course.RegionId,
                RegionName = course.CourseRegion?.RegionName // Giả sử Region có property Name
            }).ToList();

            return courseDTOs;
        }


    }
}
