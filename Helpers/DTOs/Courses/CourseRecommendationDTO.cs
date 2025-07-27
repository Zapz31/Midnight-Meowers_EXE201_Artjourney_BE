using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Courses
{
    public class CourseRecommendationDTO
    {
        public long CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public string HistoricalPeriodName { get; set; } = string.Empty;
        public bool IsPremium { get; set; }
        public string Level { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal AverageRating { get; set; }
        public int EnrollmentCount { get; set; }
        
        /// <summary>
        /// Confidence score for this recommendation (0-100)
        /// </summary>
        public int RecommendationScore { get; set; }
        
        /// <summary>
        /// Reason why this course is recommended
        /// </summary>
        public string RecommendationReason { get; set; } = string.Empty;
        
        /// <summary>
        /// Tags that matched user preferences
        /// </summary>
        public List<string> MatchedPreferences { get; set; } = new List<string>();
        
        /// <summary>
        /// Source of recommendation (Survey, Enrollment, Trending, etc.)
        /// </summary>
        public string RecommendationSource { get; set; } = string.Empty;
    }

    public class UserPreferenceProfileDTO
    {
        public long UserId { get; set; }
        public List<string> PreferredRegions { get; set; } = new List<string>();
        public List<string> PreferredHistoricalPeriods { get; set; } = new List<string>();
        public List<string> PreferredArtTypes { get; set; } = new List<string>();
        public List<string> PreferredLearningStyles { get; set; } = new List<string>();
        public List<string> PreferredArtMovements { get; set; } = new List<string>();
        public string KnowledgeLevel { get; set; } = string.Empty;
        public string LearningMotivation { get; set; } = string.Empty;
        public string PreferredTimeCommitment { get; set; } = string.Empty;
        public List<string> EnrolledCourseRegions { get; set; } = new List<string>();
        public List<string> EnrolledCoursePeriods { get; set; } = new List<string>();
        public List<string> CompletedCourseRegions { get; set; } = new List<string>();
        public List<string> CompletedCoursePeriods { get; set; } = new List<string>();
    }
}
