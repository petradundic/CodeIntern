﻿using CodeIntern.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace CodeIntern.DataAccess.Data
{
    public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {

        }
       
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<Internship> Internship { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<InternshipApplication> InternshipApplication { get; set;}
        public DbSet<SavedInternship> SavedInternship { get; set; }
        public DbSet<Notification> Notification { get; set; }

    }
}
