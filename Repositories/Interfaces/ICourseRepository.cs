using BusinessObjects.Models;
using Helpers.DTOs.Courses;
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
        public Task<List<CourseDTO>> GetAllPublishedCoursesAsync();
    }
}
