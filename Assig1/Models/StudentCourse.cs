namespace Assig1.Models
{
    public class StudentCourse
    {
        // Join entity primary key
        public int Id { get; set; }

        // Foreign key to Student
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        // Foreign key to Course
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        // Status fields required by the task
        public CourseStatus CourseStatus { get; set; } = CourseStatus.NotStarted;
        public PassStatus PassStatus { get; set; } = PassStatus.Pending;

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletionDate { get; set; }
    }
}