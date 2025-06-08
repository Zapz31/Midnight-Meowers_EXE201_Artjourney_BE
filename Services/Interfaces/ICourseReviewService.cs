using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.CourseReivew;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ICourseReviewService
    {
        public Task<ApiResponse<CourseReview>> CreateCourseReview(CreateCourseReviewRequestDTO requestDTO, long userId, string status);
    }
}
