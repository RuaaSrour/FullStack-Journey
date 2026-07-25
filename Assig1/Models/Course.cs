using System.ComponentModel.DataAnnotations;

namespace Assig1.Models
{
    public class Course
    {
        // Course primary key
        public int Id { get; set; }

        // Course name
        public string Name { get; set; } = string.Empty;

        // Course credit hours
        [Range(1, 4)]
        public int Hours { get; set; }

        // Enrollment records linking this course to students
        public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

        // Prerequisites required before taking this course
        public ICollection<CoursePrerequisite> Prerequisites { get; set; } = new List<CoursePrerequisite>();

        // Courses that require this course as a prerequisite
        public ICollection<CoursePrerequisite> RequiredFor { get; set; } = new List<CoursePrerequisite>();

        // Teachers who teach this course
        public List<Teacher> Teachers { get; set; } = new();
    }
}