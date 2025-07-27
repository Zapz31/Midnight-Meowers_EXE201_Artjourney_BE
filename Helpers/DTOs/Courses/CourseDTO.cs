using BusinessObjects.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Courses
{
    public class CourseDTO
    {
        [BindNever]
        public long CourseId { get; set; }

        [Required]
        [Display(Name = "title")] 
        public string Title { get; set; } = string.Empty;

        [Display(Name = "thumbnail_image")] 
        public IFormFile? ThumbnailImage { get; set; }

        public string? ThumbnailImageUrl { get; set; }

        [Display(Name = "description")] 
        public string? Description { get; set; }

        [Display(Name = "videos")] //--
        public List<IFormFile>? Videos { get; set; }

        [Display(Name = "course_images")] // --
        public List<IFormFile>? CourseImages { get; set; }

        [Display(Name = "course_level")] 
        public CourseLevel Level { get; set; } = CourseLevel.Easy;

        [Display(Name = "price")]
        public long Price { get; set; }

        [Display(Name = "status")]
        public CourseStatus Status { get; set; } = CourseStatus.Draft;

        [Display(Name = "historical_period_id")]
        [Required]
        public long HistoricalPeriodId { get; set; }
        public string? HistoricalPeriodName { get; set; }

        [Display(Name = "region_id")]
        [Required]
        public long RegionId { get; set; }

        public string? RegionName { get; set; }
        public string? LearningOutcomes { get; set; } // +
        public TimeSpan? EstimatedDuration { get; set; } // +
        public bool? IsPremium { get; set; } //+

        public IFormFile? CoverImage { get; set; } // +
        public string? CoverImageUrl { get; set; }
        
        // Additional properties for recommendations
        public string? ThumbnailUrl { get; set; }
        public decimal AverageRating { get; set; } = 0;
        public int EnrollmentCount { get; set; } = 0;
        public int TotalFeedbacks { get; set; } = 0;
    }
}
