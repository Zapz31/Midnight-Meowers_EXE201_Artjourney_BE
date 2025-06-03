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
        public Task<ApiResponse<List<CourseDTO>>> GetAllPublishedCoursesAsync();
    }
}
