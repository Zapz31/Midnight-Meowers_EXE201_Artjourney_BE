using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Collections;

namespace BusinessObjects.Models
{
    [Table("users")] // <--- Mapping bảng
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("user_id")]
        public long UserId { get; set; }

        [Column("fullname")]
        public string Fullname { get; set; } = string.Empty;

        [Column("role")]
        public AccountRole Role { get; set; } = AccountRole.Learner; // enum → string

        [Column("gender")]
        public Gender Gender { get; set; } = Gender.Other;

        [Column("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("password")]
        [JsonIgnore]
        public string Password { get; set; } = String.Empty;

        [Column("birthday")]
        public DateTime Birthday { get; set; } = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("banned_at")]
        public DateTime? BannedAt { get; set; }

        [Column("avatar_url")]
        public string AvatarUrl { get; set; } = "https://www.svgrepo.com/show/452030/avatar-default.svg";

        [Column("status")]
        public AccountStatus Status { get; set; } = AccountStatus.Pending;  // enum → string

        [Column("is_surveyed")]
        public bool IsSurveyed { get; set; } = false;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        //navigate properties
        public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();

        [InverseProperty("CreatedByUser")]
        public virtual ICollection<SurveyQuestion> SurveyQuestions { get; set; } = new List<SurveyQuestion>();

        [InverseProperty("User")]
        public virtual ICollection<UserOptionChoice> UserOptionChoices { get; set; } = new List<UserOptionChoice>();

        [InverseProperty("CreatedUser")]
        public virtual ICollection<Region> CreatedRegions { get; set; } = new List<Region>();

        [InverseProperty("CreatedUser")]
        public virtual ICollection<HistoricalPeriod> CreatedHistoricalPeriods { get; set; } = new List<HistoricalPeriod>();

        [InverseProperty("CreatedCourseUser")]
        public virtual ICollection<Course> CreatedCourses { get; set; } = new List<Course>();

        [InverseProperty("User")]
        public virtual ICollection<UserCourseInfo> UserCourseInfos { get; set; } = new List<UserCourseInfo>();

        [InverseProperty("UserPremium")]
        public virtual ICollection<UserPremiumInfo> UserPremiumInfos { get; set; } = new List<UserPremiumInfo>();

        [InverseProperty("ModuleCreator")]
        public virtual ICollection<Module> Modules { get; set; } = new List<Module>();

        [InverseProperty("ModuleUser")]
        public virtual ICollection<UserModuleInfo> UserModuleInfos { get; set; } = new List<UserModuleInfo>();

        [InverseProperty("LearningContentCreator")]
        public virtual ICollection<LearningContent> LearningContents { get; set; } = new List<LearningContent>();

        [InverseProperty("User")]
        public virtual ICollection<UserLearningProgress> UserLearningProgresses { get; set; } = new List<UserLearningProgress>();

        [InverseProperty("User")]
        public virtual ICollection<UserSubModuleInfo> UserSubModuleInfos { get; set; } = new List<UserSubModuleInfo>();
        
        [InverseProperty("User")]
        public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();

        [InverseProperty("User")]
        public virtual ICollection<ChallengeSession> ChallengeSessions { get; set; } = new List<ChallengeSession>();

        [InverseProperty("User")]
        public virtual ICollection<UserChallengeHighestScore> UserChallengeHighestScores { get; set; } = new List<UserChallengeHighestScore>();
    }
}
