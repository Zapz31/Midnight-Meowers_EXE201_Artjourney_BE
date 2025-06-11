using BusinessObjects.Models;
using Helpers.DTOs.Courses;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ICourseRepository
    {
        public Task<Course> CreateCourseAsync(Course course);
        public Task<List<LearnPageCourseReginDTO>> GetAllPublishedCoursesGroupedByRegionAsync();
        public Task<PaginatedResult<SearchResultCourseDTO>> SearchCoursesAsync(string? input, int pageNumber = 1, int pageSize = 10);
        public Task<bool> UpdateTotalFeedBackAndAverageRatingAsync(long courseId, int newTotalFeeback, decimal newAverageRating);
        public Task<Course?> GetSingleCourseAsync(long courseId);
        public Task UpdateCourseAsync(Course course);
        public Task<CourseLearningStatisticsDTO> GetCourseLearningStatisticsOptimizedAsync(long courseId);
    }
}
