namespace Assig1.Models
{
    public class Student
    {
        // Student primary key
        public int Id { get; set; }

        // Student name
        public string Name { get; set; } = string.Empty;

        // Enrollment records linking this student to courses
        public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
    }
}