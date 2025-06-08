using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("courses")]
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("course_id")]
        public long CourseId { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("thumbnail_url")]
        public string? ThumbnailUrl { get; set; }

        [Column("is_certificate_available")]
        public bool IsCertificateAvailable { get; set; } = false;

        [Column("is_featured")]
        public bool IsFeatured { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("level")]
        public CourseLevel Level { get; set; } = CourseLevel.Easy;

        [Column("learning_outcomes")]
        public string? LearningOutcomes { get; set; }

        [Column("promo_video_url")]
        public string? PromoVideoUrl { get; set; }

        [Column("cover_image_url")]
        public string? CoverImageUrl { get; set; }

        [Column("price")]
        public long Price { get; set; }

        [Column("status")]
        public CourseStatus Status { get; set; } = CourseStatus.Draft;

        [Column("enrollment_count")]
        public long EnrollmentCount { get; set; } = 0;

        [Column("is_premium")]
        public bool IsPremium { get; set; } = false;

        [Column("published_at")]
        public DateTime? PublishedAt { get; set; }

        [Column("estimated_duration")]
        public TimeSpan? EstimatedDuration { get; set; }

        [Column("updated_by")]
        public long? UpdatedBy { get; set; }

        [Column("archived_by")]
        public long? ArchivedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("archived_at")]
        public DateTime? ArchivedAt { get; set; }

        [Column("prerequisite_course_id")]
        public long? PrerequisiteCourseId { get; set; }

        [Column("total_rating")]
        public int TotalRating { get; set; } = 0;

        [Column("average_rating", TypeName = "decimal(3,2)")]
        public decimal AverageRating { get; set; } = 0;

        [Column("total_feedback")]
        public int TotalFeedbacks { get; set; } = 0;

        // navigation properties
        [Column("created_by")]
        [ForeignKey("CreatedCourseUser")]
        public long CreatedBy { get; set; }

        public virtual User CreatedCourseUser { get; set; } = null!;

        [Column("historical_period_id")]
        [ForeignKey("CourseHistoricalPeriod")]
        public long HistoricalPeriodId { get; set; }
        public virtual HistoricalPeriod CourseHistoricalPeriod { get; set; } = null!;

        [Column("region_id")]
        [ForeignKey("CourseRegion")]
        public long RegionId { get; set; }
        public virtual Region CourseRegion { get; set; } = null!;

        // 1 -N 
        [InverseProperty("Course")]
        public virtual ICollection<UserCourseInfo> UserCourseInfos { get; set; } = new List<UserCourseInfo>();

        [InverseProperty("ModuleCourse")]
        public virtual ICollection<Module> Modules { get; set; } = new List<Module>();

    }
}

/*
Table courses {
  course_id long [primary key] --
  title varchar --
  thumbnail_url varchar //--thumbnail của khóa học --
  is_certificate_available bool // --Đánh dấu khóa học có cấp chứng chỉ --
  is_featured bool // --Đánh dấu khóa học nổi bật --
  description varchar --
  level varchar --
  learning_outcomes json // -- lưu data cho mục while should you learn --
  promo_video_url varchar // URL của video giới thiệu khóa học ==
  cover_image_url varchar//-- Ảnh bìa của khóa học ==
  price long //--giá tiền mua lẻ của một course ==
  status varchar //-- 'Draft', 'Published', 'Archived' ==
  enrollment_count long //-- số lượng thành viên tham gia course này ==
  is_premium bool //-- Đánh dấu xem khóa học có phải là premium hay không. (chỉ được sử dụng bởi gói tháng) ==
  published_at datetime //-- Thời gian khóa học này được published bởi admin ==
  estimated_duration timestamp ==
  created_by long // --id của người tạo ra khóa học này ==
  updated_by long // --id của người đã cập nhật bất kỳ thông tin nào đó trong khóa học này ==
  archived_by long // -- id của người đã archive khóa học này ==
  created_at datetime ==
  updated_at datetime ==
  archived_at datetime ==
  //navigation
  historical_period_id long [ref: > historical_periods.historical_period_id] ==
  region_id long [ref: > regions.region_id] ==
  prerequisite_course_id long // id của course tiên quyết ==
}
*/