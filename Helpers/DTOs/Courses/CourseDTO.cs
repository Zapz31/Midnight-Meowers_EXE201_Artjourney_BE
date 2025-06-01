using Microsoft.AspNetCore.Http;
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
        [Display(Name = "title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "thumbnail_url")]
        public string? ThumbnailUrl { get; set; }

        [Display(Name = "description")]
        public string? Description { get; set; }

        [Display(Name = "videos")]
        public List<IFormFile>? Videos { get; set; }

        [Display(Name = "course_images")]
        public List<IFormFile>? CourseImages { get; set; }
    }
}
