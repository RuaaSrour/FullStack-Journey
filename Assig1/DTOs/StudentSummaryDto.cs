namespace Assig1.DTOs
{
    // Minimal student shape used when nested inside Course relationship responses
    public class StudentSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
