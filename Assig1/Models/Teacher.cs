namespace Assig1.Models
{
    public class Teacher
    {
        // Teacher primary key
        public int Id { get; set; }

        // Teacher name
        public string Name { get; set; } = string.Empty;

        // Courses taught by the teacher
        public List<Course> Courses { get; set; } = new();
    }
}