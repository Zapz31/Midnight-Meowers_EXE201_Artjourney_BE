﻿using BusinessObjects.Models;
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
        }

    }
}
