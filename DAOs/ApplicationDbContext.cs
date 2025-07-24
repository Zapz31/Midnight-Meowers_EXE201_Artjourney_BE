using BusinessObjects.Models;
using Helpers.DTOs.CourseReivew;
using Helpers.DTOs.Courses;
using Helpers.DTOs.General;
using Helpers.DTOs.Module;
using Helpers.DTOs.SubModule;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAOs
{
    public class ApplicationDbContext : DbContext
    {
        /*
         use those commands when you want update database 
        dotnet ef migrations add InitCreate --project DAOs --startup-project Artjouney_BE
        dotnet ef database update --project DAOs --startup-project Artjouney_BE
        dotnet ef migrations remove --project DAOs --startup-project Artjouney_BE

         */
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        //table declare
        public DbSet<User> Users { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }
        public DbSet<VerificationInfo> VerificationInfos { get; set; }
        public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
        public DbSet<SurveyOption> SurveyOptions { get; set; }
        public DbSet<UserOptionChoice> UserOptionChoices { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<HistoricalPeriod> HistoricalPeriods { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<UserCourseInfo> UserCourseInfos { get; set; }
        public DbSet<UserPremiumInfo> UserPremiumInfos { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<UserModuleInfo> UserModuleInfos { get; set; }
        public DbSet<SubModule> SubModules { get; set; }
        public DbSet<LearningContent> LearningContents { get; set; }
        public DbSet<ChallengeItem> ChallengeItems { get; set; }
        public DbSet<UserLearningProgress> UserLearningProgresses { get; set; }
        public DbSet<UserCourseStreak> userCourseStreaks { get; set; }
        public DbSet<CourseReview> CourseReviews { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<UserSubModuleInfo> UserSubModuleInfos { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
         public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOptions> QuestionOptions { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<Artwork> Artworks { get; set; }
        public DbSet<ArtworkDetail> ArtworkDetails { get; set; }
        public DbSet<ChallengeSession> ChallengeSessions { get; set; }
        public DbSet<UserChallengeHighestScore> UserChallengeHighestScores { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        
        // DTO
        public DbSet<CourseDetailScreenFlat> CourseDetailScreenFlats { get; set; }
        public DbSet<BasicCourseReviewFlatResponseDTO> BasicCourseReviewFlatResponseDTOs {  get; set; } 
        public DbSet<QueryResultA> queryResultAs {  get; set; }
        public DbSet<ModuleSubModuleCourseIds> ModuleSubModuleCourseIds { get; set; }
        public DbSet<QueryResultBFlat> QueryResultBFlats { get; set; }
        public DbSet<ModuleCourseHasEnrolledBasicViewDTO> ModuleCourseHasEnrolledBasicViewDTOs { get; set; }
        public DbSet<SubModuleCourseHasEnrolledBasicViewDTO> SubModuleCourseHasEnrolledBasicViewDTOs { get; set; }
        public DbSet<TotalScoreResult> TotalScoreResults { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure DateTime properties to handle UTC conversion for PostgreSQL
            modelBuilder.Entity<User>()
                .Property(u => u.Birthday)
                .HasConversion(
                    v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                    v => v);
            
            modelBuilder.Entity<Order>().ToTable("orders");
            
            // Chat table configurations
            modelBuilder.Entity<ChatSession>();
            modelBuilder.Entity<ChatMessage>();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.Gender)
                .HasConversion<string>();

            modelBuilder.Entity<LoginHistory>()
                .Property(l => l.LoginResult)
                .HasConversion<string>();

            modelBuilder.Entity<Course>()
                .Property(c => c.Level)
                .HasConversion<string>();

            modelBuilder.Entity<Course>()
                .Property(c => c.Status)
                .HasConversion<string>();

            modelBuilder.Entity<UserCourseInfo>()
                .Property(c => c.EnrollmentStatus)
                .HasConversion<string>();

            modelBuilder.Entity<UserCourseInfo>()
                .Property(c => c.LearningStatus)
                .HasConversion<string>();

            modelBuilder.Entity<UserPremiumInfo>()
                .Property(upi => upi.Status)
                .HasConversion<string>();

            modelBuilder.Entity<LearningContent>()
                .Property(lc => lc.ContentType)
                .HasConversion<string>();

            modelBuilder.Entity<LearningContent>()
                .Property(lc => lc.ChallengeType)
                .HasConversion<string>();

            modelBuilder.Entity<ChallengeItem>()
                .Property(ci => ci.ItemTypes)
                .HasConversion<string>();

            modelBuilder.Entity<UserLearningProgress>()
                .Property(ulp => ulp.Status)
                .HasConversion<string>();

            modelBuilder.Entity<SurveyQuestion>()
            .HasOne(sq => sq.CreatedByUser)
            .WithMany(u => u.SurveyQuestions)
            .HasForeignKey(sq => sq.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SurveyOption>()
            .HasOne(so => so.SurveyQuestion)
            .WithMany(sq => sq.SurveyOptions)
            .HasForeignKey(so => so.SurveyQuestionId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserOptionChoice>()
                .HasOne(uoc => uoc.User)
                .WithMany(u => u.UserOptionChoices)
                .HasForeignKey(uoc => uoc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserOptionChoice>()
                .HasOne(uoc => uoc.SurveyOption)
                .WithMany(so => so.UserOptionChoices)
                .HasForeignKey(uoc => uoc.SurveyOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Region>()
                .HasOne(r => r.CreatedUser)
                .WithMany(u => u.CreatedRegions)
                .HasForeignKey(r => r.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HistoricalPeriod>()
                .HasOne(hp => hp.CreatedUser)
                .WithMany(u => u.CreatedHistoricalPeriods)
                .HasForeignKey(r => r.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.CreatedCourseUser)
                .WithMany(u => u.CreatedCourses)
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.CourseHistoricalPeriod)
                .WithMany(hp => hp.Courses)
                .HasForeignKey(c => c.HistoricalPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.CourseRegion)
                .WithMany(r => r.Courses)
                .HasForeignKey(c => c.RegionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserCourseInfo>()
                .HasOne(uci => uci.User)
                .WithMany(u => u.UserCourseInfos)
                .HasForeignKey(uci => uci.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserCourseInfo>()
                .HasOne(uci => uci.Course)
                .WithMany(c => c.UserCourseInfos)
                .HasForeignKey(uci => uci.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserPremiumInfo>()
                .HasOne(upi => upi.UserPremium)
                .WithMany(u => u.UserPremiumInfos)
                .HasForeignKey(upi => upi.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Module>()
                .HasOne(m => m.ModuleCourse)
                .WithMany(c => c.Modules)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Module>()
                .HasOne(m => m.ModuleCreator)
                .WithMany(u => u.Modules)
                .HasForeignKey(m => m.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserModuleInfo>()
                .HasOne(umi => umi.ModuleUser)
                .WithMany(u => u.UserModuleInfos)
                .HasForeignKey(umi => umi.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserModuleInfo>()
                .HasOne(umi => umi.Module)
                .WithMany(m => m.UserModuleInfos)
                .HasForeignKey(umi => umi.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SubModule>()
                .HasOne(sm => sm.Module)
                .WithMany(m => m.SubModules)
                .HasForeignKey(sm => sm.ModuleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<LearningContent>()
                .HasOne(lc => lc.LearningContentCreator)
                .WithMany(u => u.LearningContents)
                .HasForeignKey(lc => lc.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<LearningContent>()
                .HasOne(lc => lc.ContentSubModule)
                .WithMany(sm => sm.LearningContents)
                .HasForeignKey(lc => lc.SubModuleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<LearningContent>()
                .HasOne(lc => lc.Course)
                .WithMany(c => c.LearningContents)
                .HasForeignKey(lc => lc.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ChallengeItem>()
                .HasOne(ci => ci.LearningContent)
                .WithMany(lc => lc.ChallengeItems)
                .HasForeignKey(ci => ci.LearningContentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserLearningProgress>()
                .HasOne(ulp => ulp.User)
                .WithMany(u => u.UserLearningProgresses)
                .HasForeignKey(ulp => ulp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserLearningProgress>()
                .HasOne(ulp => ulp.LearningContent)
                .WithMany(lc => lc.UserLearningProgresses)
                .HasForeignKey(ulp => ulp.LearningContentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RegionHisoricalPeriod>()
                .HasOne(rhp => rhp.Region)
                .WithMany(r => r.RegionHisoricalPeriods)
                .HasForeignKey(rhp => rhp.RegionId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RegionHisoricalPeriod>()
                .HasOne(rhp => rhp.HistoricalPeriod)
                .WithMany(hp => hp.RegionHisoricalPeriods)
                .HasForeignKey(rhp => rhp.HistoricalPeriodId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserCourseStreak>()
                .Property(cs => cs.LastAccessDate)
                .HasConversion(
                    v => DateTime.SpecifyKind(v.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                    v => DateOnly.FromDateTime(v)
                );

            modelBuilder.Entity<UserSubModuleInfo>()
                .HasOne(usmi => usmi.User)
                .WithMany(u => u.UserSubModuleInfos)
                .HasForeignKey(usmi => usmi.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserSubModuleInfo>()
                .HasOne(usmi => usmi.SubModule)
                .WithMany(sm => sm.UserSubModuleInfos)
                .HasForeignKey(usmi => usmi.SubModuleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<QuizAttempt>()
                .HasOne(qa => qa.LearningContent)
                .WithMany(lc => lc.QuizAttempts)
                .HasForeignKey(qa => qa.LearningContentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<QuizAttempt>()
                .HasOne(qa => qa.User)
                .WithMany(u => u.QuizAttempts)
                .HasForeignKey(qa => qa.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.LearningContent)
                .WithMany(lc => lc.Questions)
                .HasForeignKey(q => q.LearningContentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<QuestionOptions>()
                .HasOne(qo => qo.Question)
                .WithMany(q => q.QuestionOptions)
                .HasForeignKey(qo => qo.QuestionId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.QuizAttempt)
                .WithMany(qa => qa.UserAnswers)
                .HasForeignKey(ua => ua.QuizAttemptId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Question)
                .WithMany(q => q.UserAnswers)
                .HasForeignKey(ua => ua.QuestionId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.SelectedOption)
                .WithMany(qo => qo.UserAnswers)
                .HasForeignKey(ua => ua.SelectedOptionId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Challenge>()
                .HasOne(cl => cl.Course)
                .WithMany(c => c.Challenges)
                .HasForeignKey(cl => cl.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Artwork>()
                .HasOne(aw => aw.Challenge)
                .WithMany(cl => cl.Artworks)
                .HasForeignKey(aw => aw.ChallengeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ArtworkDetail>()
                .HasOne(awd => awd.Artwork)
                .WithMany(aw => aw.ArtworkDetails)
                .HasForeignKey(awd => awd.ArtworkId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChallengeSession>()
                .HasOne(cs => cs.User)
                .WithMany(u => u.ChallengeSessions)
                .HasForeignKey(cs => cs.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChallengeSession>()
                .HasOne(cs => cs.Challenge)
                .WithMany(cl => cl.ChallengeSessions)
                .HasForeignKey(cs => cs.ChallengeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserChallengeHighestScore>()
                .HasOne(uchs => uchs.User)
                .WithMany(u => u.UserChallengeHighestScores)
                .HasForeignKey(uchs => uchs.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserChallengeHighestScore>()
                .HasOne(uchs => uchs.Challenge)
                .WithMany(c => c.UserChallengeHighestScores)
                .HasForeignKey(uchs => uchs.ChallengeId)
                .OnDelete(DeleteBehavior.Cascade);

            //dto
            modelBuilder.Entity<CourseDetailScreenFlat>().HasNoKey();
            modelBuilder.Entity<BasicCourseReviewFlatResponseDTO>().HasNoKey();
            modelBuilder.Entity<QueryResultA>().HasNoKey();
            modelBuilder.Entity<ModuleSubModuleCourseIds>().HasNoKey();
            modelBuilder.Entity<QueryResultBFlat>().HasNoKey();
            modelBuilder.Entity<ModuleCourseHasEnrolledBasicViewDTO>().HasNoKey();
            modelBuilder.Entity<SubModuleCourseHasEnrolledBasicViewDTO>().HasNoKey();
            modelBuilder.Entity<TotalScoreResult>().HasNoKey();
        }

        public override int SaveChanges()
        {
            FixDateTimeKinds();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            FixDateTimeKinds();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void FixDateTimeKinds()
        {
            var entities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entity in entities)
            {
                var properties = entity.Properties
                    .Where(p => p.CurrentValue is DateTime dateTime && dateTime.Kind == DateTimeKind.Unspecified);

                foreach (var property in properties)
                {
                    if (property.CurrentValue is DateTime currentDateTime)
                    {
                        property.CurrentValue = DateTime.SpecifyKind(currentDateTime, DateTimeKind.Utc);
                    }
                }
            }
        }

    }
}
