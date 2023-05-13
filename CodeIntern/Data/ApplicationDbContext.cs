using CodeIntern.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeIntern.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {

        }
        public DbSet<Student> Student { get; set; }
        public DbSet<Company> Company { get; set; }
    }
}
