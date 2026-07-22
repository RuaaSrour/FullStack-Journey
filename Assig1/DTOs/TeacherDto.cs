using System.ComponentModel.DataAnnotations;

namespace Assig1.DTOs
{
    // Returned from GET endpoints
    public class TeacherDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Accepted by POST - no Id, no navigation collections
    public class TeacherCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    // Accepted by PUT - no Id, no navigation collections
    public class TeacherUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    // Returned from GET /api/teachers/{teacherId}/courses
    public class TeacherCoursesDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<CourseSummaryDto> Courses { get; set; } = new();
    }
}
