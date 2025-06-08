using BusinessObjects.Models;
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
        public CourseReviewRepository(IUnitOfWork unitOfWork, ICourseRepository courseRepository)
        {
            _unitOfWork = unitOfWork;
            _courseRepository = courseRepository;
        }

        public async Task<CourseReview> CreateCourseReview(CourseReview courseReview)
        {
                var createdCourseReview = await _unitOfWork.GetRepo<CourseReview>().CreateAsync(courseReview);
                await _unitOfWork.SaveChangesAsync();
                return createdCourseReview;
        }

    }
}
