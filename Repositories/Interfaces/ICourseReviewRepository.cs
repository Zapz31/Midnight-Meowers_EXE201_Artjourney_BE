using BusinessObjects.Models;
using Helpers.DTOs.CourseReivew;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ICourseReviewRepository
    {
        public Task<CourseReview> CreateCourseReview(CourseReview courseReview);
        public Task<List<BasicCourseReviewFlatResponseDTO>> GetBasicCourseReviewFlatResponseDTOsByCourseId(long courseId);
    }
}
