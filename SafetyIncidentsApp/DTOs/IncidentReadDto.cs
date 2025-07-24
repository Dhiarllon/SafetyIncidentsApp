using SafetyIncidentsApp.Models;

namespace SafetyIncidentsApp.DTOs
{
    public class IncidentReadDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IncidentType Type { get; set; }
        public SeverityLevel Severity { get; set; }
        public string? CorrectiveAction { get; set; }
        public bool IsResolved { get; set; } = false;
        public DateTime? ResolvedDate { get; set; }
        
        // Essential fields
        public Guid ReportedById { get; set; }
        public string ReportedByName { get; set; } = string.Empty;
        public Guid? InvolvedEmployeeId { get; set; }
        public string? InvolvedEmployeeName { get; set; }
        public IncidentStatus Status { get; set; }
        public bool RequiresManagerApproval { get; set; } = false;
        public int EstimatedCost { get; set; } = 0;
    }
}
