using BusinessObjects.Models;
using Repositories.Interfaces;
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
    }
}
