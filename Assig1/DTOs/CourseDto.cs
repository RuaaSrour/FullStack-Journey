using System.ComponentModel.DataAnnotations;

namespace Assig1.DTOs
{
    // Returned from GET endpoints
    public class CourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Hours { get; set; }
    }

    // Accepted by POST - no Id, no navigation collections
    public class CourseCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 4)]
        public int Hours { get; set; }
    }

    // Accepted by PUT - no Id, no navigation collections
    public class CourseUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 4)]
        public int Hours { get; set; }
    }

    // Returned from GET /api/courses/{courseId}/students
    public class CourseStudentsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Hours { get; set; }
        public List<StudentSummaryDto> Students { get; set; } = new();
    }

    // Returned from GET /api/courses/{courseId}/teachers
    public class CourseTeachersDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Hours { get; set; }
        public List<TeacherSummaryDto> Teachers { get; set; } = new();
    }
}
