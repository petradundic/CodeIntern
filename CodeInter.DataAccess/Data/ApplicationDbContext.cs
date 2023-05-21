using CodeIntern.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace CodeIntern.DataAccess.Data
{
    public class ApplicationDbContext: IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {

        }
       
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<Internship> Internship { get; set; }
        public DbSet<Company> Company { get; set; }

        public DbSet<InternApplication> InetrnApplication { get; set; } 
    }
}
