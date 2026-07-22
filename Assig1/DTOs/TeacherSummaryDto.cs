namespace Assig1.DTOs
{
    // Minimal teacher shape used when nested inside Course relationship responses
    public class TeacherSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
