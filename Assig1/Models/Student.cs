namespace Assig1.Models
{
    public class Student
    {
        // Student primary key
        public int Id { get; set; }

        // Student name
        public string Name { get; set; } = string.Empty;

        // Courses registered by the student
        public List<Course> Courses { get; set; } = new();
    }
}