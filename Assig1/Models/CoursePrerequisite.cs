namespace Assig1.Models
{
    public class CoursePrerequisite
    {
        // Join entity primary key
        public int Id { get; set; }

        // The course that has the prerequisite
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        // The course required beforehand
        public int PrerequisiteCourseId { get; set; }
        public Course PrerequisiteCourse { get; set; } = null!;
    }
}