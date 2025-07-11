namespace SafetyIncidentsApp.Models
{
    public class SafetyInspection
    {
        public Guid Id { get; set; }
        public DateTime InspectionDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string InspectorName { get; set; } = string.Empty;
        public Guid InspectorId { get; set; }
        public InspectionType Type { get; set; }
        public InspectionStatus Status { get; set; }
        public int RiskScore { get; set; }
        public string Findings { get; set; } = string.Empty;
        public string? Recommendations { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public bool RequiresFollowUp { get; set; }
        
        // Navigation properties
        public virtual Employee Inspector { get; set; } = null!;
        public virtual ICollection<Incident> RelatedIncidents { get; set; } = new List<Incident>();
    }
    
    public enum InspectionType
    {
        Routine,
        IncidentFollowUp,
        Compliance,
        Emergency,
        PreShift
    }
    
    public enum InspectionStatus
    {
        Scheduled,
        InProgress,
        Completed,
        RequiresAction,
        Closed
    }
} 