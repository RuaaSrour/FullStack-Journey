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

        // Students registered in the course
        public List<Student> Students { get; set; } = new();

        // Teachers who teach this course
        public List<Teacher> Teachers { get; set; } = new();
    }
}