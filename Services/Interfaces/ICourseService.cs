using BusinessObjects.Models;
using Helpers.DTOs.Courses;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ICourseService
    {
        public Task<ApiResponse<Course>> CreateCourse(CourseDTO courseDTO);
        public Task<ApiResponse<List<LearnPageCourseReginDTO>>> GetAllPublishedCoursesGroupedByRegionAsync();
        public Task<ApiResponse<PaginatedResult<SearchResultCourseDTO>>> SearchCoursesAsync(string? input, int pageNumber = 1, int pageSize = 10);
        public Task<ApiResponse<CourseDetailResponseDTO>> GetCourseDetailAsync(long courseId);
        public Task<ApiResponse<CourseDetailResponseDTO>> GetCourseDetailForGuestAsync(long courseId);
        public Task<ApiResponse<List<CourseHasEnrolledBasicViewReponseDTO>>> GetCoursesHasEnrolledByUserIdAsync(long userId);
        public Task<ApiResponse<bool>> DeleteCourseAsync(long courseId, long? deletedBy = null);
    }
}
