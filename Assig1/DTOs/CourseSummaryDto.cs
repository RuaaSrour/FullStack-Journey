namespace Assig1.DTOs
{
    // Minimal course shape used when nested inside Student/Teacher relationship responses
    public class CourseSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Hours { get; set; }
    }
}
