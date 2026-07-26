using Assig1.Models;

namespace Assig1.DTOs
{
    // Accepted by PUT /api/students/{studentId}/courses/{courseId}
    public class StudentCourseUpdateDto
    {
        public CourseStatus CourseStatus { get; set; }
        public PassStatus PassStatus { get; set; }
        public DateTime? CompletionDate { get; set; }
    }

    // Returned from GET /api/student-courses/search
    public class StudentCourseSearchResultDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public CourseStatus CourseStatus { get; set; }
        public PassStatus PassStatus { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime? CompletionDate { get; set; }
    }
}
