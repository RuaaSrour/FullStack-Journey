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

        // Represent the StudentCourses table
        public DbSet<StudentCourse> StudentCourses { get; set; }

        // Represent the CoursePrerequisites table
        public DbSet<CoursePrerequisite> CoursePrerequisites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // StudentCourse: two FKs to different tables, EF can mostly infer this,
            // but we're explicit for clarity and to attach the unique constraint.
            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Student)
                .WithMany(s => s.StudentCourses)
                .HasForeignKey(sc => sc.StudentId);

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany(c => c.StudentCourses)
                .HasForeignKey(sc => sc.CourseId);

            // Prevents the same student from being enrolled in the same course twice
            modelBuilder.Entity<StudentCourse>()
                .HasIndex(sc => new { sc.StudentId, sc.CourseId })
                .IsUnique();

            // CoursePrerequisite: two FKs to the SAME table, so we must tell EF
            // which navigation goes with which FK, and disable cascade delete
            // on both sides to avoid SQL Server's multiple-cascade-path error.
            modelBuilder.Entity<CoursePrerequisite>()
                .HasOne(cp => cp.Course)
                .WithMany(c => c.Prerequisites)
                .HasForeignKey(cp => cp.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CoursePrerequisite>()
                .HasOne(cp => cp.PrerequisiteCourse)
                .WithMany(c => c.RequiredFor)
                .HasForeignKey(cp => cp.PrerequisiteCourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevents the same prerequisite pair from being added twice
            modelBuilder.Entity<CoursePrerequisite>()
                .HasIndex(cp => new { cp.CourseId, cp.PrerequisiteCourseId })
                .IsUnique();
        }
    }


}