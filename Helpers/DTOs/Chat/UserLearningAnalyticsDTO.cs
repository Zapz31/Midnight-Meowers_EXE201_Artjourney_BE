namespace Helpers.DTOs.Chat
{
    public class UserLearningAnalyticsDTO
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime AnalyticsTimestamp { get; set; } = DateTime.UtcNow;
        
        // Learning Progress Overview
        public LearningOverviewDTO LearningOverview { get; set; } = new();
        
        // Course-specific details
        public List<DetailedCourseProgressDTO> CourseProgress { get; set; } = new();
        
        // Learning patterns and preferences
        public LearningPatternsDTO LearningPatterns { get; set; } = new();
        
        // Recommendations and next steps
        public RecommendationsDTO Recommendations { get; set; } = new();
    }
    
    public class LearningOverviewDTO
    {
        public int TotalCoursesEnrolled { get; set; }
        public int TotalCoursesCompleted { get; set; }
        public double OverallProgressPercentage { get; set; }
        public int TotalStudyDays { get; set; }
        public int CurrentLongestStreak { get; set; }
        public int TotalLearningContentCompleted { get; set; }
        public TimeSpan TotalStudyTime { get; set; }
        public double AverageScorePercentage { get; set; }
    }
    
    public class DetailedCourseProgressDTO
    {
        public long CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseDescription { get; set; } = string.Empty;
        public double ProgressPercentage { get; set; }
        public DateTime EnrolledDate { get; set; }
        public DateTime? LastStudyDate { get; set; }
        public int StudyStreak { get; set; }
        public TimeSpan TotalTimeSpent { get; set; }
        
        // Current position
        public string CurrentModule { get; set; } = string.Empty;
        public string CurrentSubModule { get; set; } = string.Empty;
        public int ModulesCompleted { get; set; }
        public int TotalModules { get; set; }
        
        // Performance metrics
        public double AverageScore { get; set; }
        public int TotalAttempts { get; set; }
        public List<string> RecentlyCompletedTopics { get; set; } = new();
        public List<string> UpcomingTopics { get; set; } = new();
        public List<string> DifficultTopics { get; set; } = new();
        
        // Course-specific interests
        public string ArtPeriod { get; set; } = string.Empty;
        public string GeographicalRegion { get; set; } = string.Empty;
    }
    
    public class LearningPatternsDTO
    {
        public string PreferredStudyTime { get; set; } = string.Empty; // Morning, Afternoon, Evening
        public string LearningStyle { get; set; } = string.Empty; // Quick, Deep, Balanced
        public double AverageSessionDuration { get; set; } // in minutes
        public int WeeklyStudyFrequency { get; set; }
        public List<string> StrongAreas { get; set; } = new();
        public List<string> WeakAreas { get; set; } = new();
        public string MotivationLevel { get; set; } = string.Empty; // High, Medium, Low based on streak and consistency
    }
    
    public class RecommendationsDTO
    {
        public List<string> NextTopicsToStudy { get; set; } = new();
        public List<string> TopicsToReview { get; set; } = new();
        public List<string> SkillsToFocus { get; set; } = new();
        public string StudyScheduleSuggestion { get; set; } = string.Empty;
        public List<string> MotivationalInsights { get; set; } = new();
        public string OptimalStudyPlan { get; set; } = string.Empty;
    }
}
