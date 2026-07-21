using Assig1.Models;
using Microsoft.EntityFrameworkCore;

namespace Assig1.Data
{
    public class AppDbContext : DbContext
    {
        // Receive database settings
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Represent the Courses table
        public DbSet<Course> Courses { get; set; }

        // Represent the Students table
        public DbSet<Student> Students { get; set; }

        // Represent the Teachers table
        public DbSet<Teacher> Teachers { get; set; }


    }


}