using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.CourseReivew;
using Helpers.DTOs.Courses;
using Helpers.HelperClasses;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implements
{
    public class CourseReviewService : ICourseReviewService
    {
        private readonly ICourseReviewRepository _courseReviewRepository;
        private readonly ILogger<CourseReviewService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICourseRepository _courseRepository;

        public CourseReviewService(
            ICourseReviewRepository courseReviewRepository, 
            ILogger<CourseReviewService> logger,
            IUnitOfWork unitOfWork,
            ICourseRepository courseRepository)
        {
            _courseReviewRepository = courseReviewRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _courseRepository = courseRepository;
        }

        public async Task<ApiResponse<CourseReview>> CreateCourseReview(CreateCourseReviewRequestDTO requestDTO, long userId, string status)
        {
            try
            {
                _logger.LogInformation("Start at CreateCourseReview at CourseReviewService.cs");
                await _unitOfWork.BeginTransactionAsync();
                if (status.Equals(AccountStatus.Suspended.ToString()))
                {
                    return new ApiResponse<CourseReview>
                    {
                        Status = ResponseStatus.Error,
                        Code = 401
                    };
                }

                CourseReview courseReview = new()
                {
                    UserId = userId,
                    CourseId = requestDTO.CourseId,
                    Rating = requestDTO.Rating,
                    Feedback = requestDTO.FeedBack,
                    CreatedAt = DateTime.UtcNow,
                    IsApproved = true
                };
                var data = await _courseReviewRepository.CreateCourseReview(courseReview);
                var course = await _courseRepository.GetSingleCourseAsync(courseReview.CourseId);
                if (course == null)
                {
                    _logger.LogError("course = null retrive by GetSingleCourseAsync in CreateCourseReview at CourseReviewService");
                    return new ApiResponse<CourseReview>
                    { 
                        Status = ResponseStatus.Error, 
                        Code = 400,
                        Message = $"Course with id {courseReview.CourseId} dosen't exist"
                    };
                }

                course.TotalRating = course.TotalRating + courseReview.Rating;
                course.TotalFeedbacks++;
                course.AverageRating = (decimal) course.TotalRating / course.TotalFeedbacks;

                // update course 
                await _courseRepository.UpdateCourseAsync(course);

                await _unitOfWork.CommitTransactionAsync();
                return new ApiResponse<CourseReview>
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = data,
                    Message = "Create course review success"
                };
            } catch (Exception ex)
            {
                _logger.LogError("Error at CreateCourseReview at CourseReviewService.cs: {ex}", ex.Message);
                await _unitOfWork.RollBackAsync();
                return new ApiResponse<CourseReview>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Message = ex.Message
                };
            }
        }
    }
}
