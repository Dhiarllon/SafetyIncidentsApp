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
        public string? ResolvedBy { get; set; }
        
        // Novos campos para regras de negócio mais complexas
        public Guid ReportedById { get; set; }
        public Guid? InvolvedEmployeeId { get; set; }
        public Guid? SafetyInspectionId { get; set; }
        public IncidentStatus Status { get; set; } = IncidentStatus.Reported;
        public bool RequiresManagerApproval { get; set; } = false;
        public bool RequiresSafetyReview { get; set; } = false;
        public DateTime? ManagerApprovalDate { get; set; }
        public string? ManagerApprovedBy { get; set; }
        public DateTime? SafetyReviewDate { get; set; }
        public string? SafetyReviewedBy { get; set; }
        public string? InvestigationNotes { get; set; }
        public int EstimatedCost { get; set; } = 0;
        public bool IsNearMiss { get; set; } = false;
        public string? Witnesses { get; set; }
        
        // Navigation properties
        public virtual Employee ReportedBy { get; set; } = null!;
        public virtual Employee? InvolvedEmployee { get; set; }
        public virtual SafetyInspection? SafetyInspection { get; set; }
    }
    
    public enum IncidentStatus
    {
        Reported,
        UnderInvestigation,
        PendingApproval,
        Approved,
        Closed,
        RequiresFollowUp
    }
}
