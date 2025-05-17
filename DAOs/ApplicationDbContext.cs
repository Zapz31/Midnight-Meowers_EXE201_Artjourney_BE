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
        }

    }
}
