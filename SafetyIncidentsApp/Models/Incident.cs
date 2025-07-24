namespace SafetyIncidentsApp.Models
{
    public class Incident
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
        
        // Essential fields for business rules
        public Guid ReportedById { get; set; }
        public Guid? InvolvedEmployeeId { get; set; }
        public IncidentStatus Status { get; set; } = IncidentStatus.Reported;
        public bool RequiresManagerApproval { get; set; } = false;
        public int EstimatedCost { get; set; } = 0;
        
        // Navigation properties
        public virtual Employee ReportedBy { get; set; } = null!;
        public virtual Employee? InvolvedEmployee { get; set; }
    }
    
    public enum IncidentStatus
    {
        Reported,
        PendingApproval,
        Approved,
        Closed
    }
}
