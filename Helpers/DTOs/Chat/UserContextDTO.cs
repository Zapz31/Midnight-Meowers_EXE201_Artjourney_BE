namespace Helpers.DTOs.Chat
{
    public class UserContextDTO
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        // Current learning state
        public List<CourseProgressDTO> EnrolledCourses { get; set; } = new List<CourseProgressDTO>();
        public List<string> CompletedTopics { get; set; } = new List<string>();
        public List<string> StruggleAreas { get; set; } = new List<string>();
        
        // Learning preferences
        public string PreferredLearningStyle { get; set; } = string.Empty;
        public List<string> InterestAreas { get; set; } = new List<string>();
        
        // Current session info
        public DateTime CurrentDateTime { get; set; } = DateTime.UtcNow;
        public string TimeZone { get; set; } = "UTC";
    }
    
    public class CourseProgressDTO
    {
        public long CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public double ProgressPercentage { get; set; }
        public string CurrentModule { get; set; } = string.Empty;
        public DateTime LastAccessed { get; set; }
        public int CurrentStreak { get; set; }
        public List<string> RecentTopics { get; set; } = new List<string>();
    }
}
