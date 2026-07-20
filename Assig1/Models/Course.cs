using System.ComponentModel.DataAnnotations;

namespace Assig1.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Range(1, 4)]
        public int Hours { get; set; }
    }
}