using BusinessObjects.Models;
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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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

        }

    }
}
