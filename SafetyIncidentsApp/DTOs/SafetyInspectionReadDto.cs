using SafetyIncidentsApp.Models;

namespace SafetyIncidentsApp.DTOs
{
    public class SafetyInspectionReadDto
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
    }
} 