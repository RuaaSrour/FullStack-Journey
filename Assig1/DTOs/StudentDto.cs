using System.ComponentModel.DataAnnotations;

namespace Assig1.DTOs
{
    // Returned from GET endpoints
    public class StudentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Accepted by POST - no Id, no navigation collections
    public class StudentCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    // Accepted by PUT - no Id, no navigation collections
    public class StudentUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    // Returned from GET /api/students/{studentId}/courses
    public class StudentCoursesDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<CourseSummaryDto> Courses { get; set; } = new();
    }
}
