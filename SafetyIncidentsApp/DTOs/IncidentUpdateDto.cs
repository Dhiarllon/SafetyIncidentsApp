using System.ComponentModel.DataAnnotations;
using SafetyIncidentsApp.Models;

namespace SafetyIncidentsApp.DTOs
{
    public class IncidentUpdateDto
    {
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        public string? Location { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        public IncidentType? Type { get; set; }

        public SeverityLevel? Severity { get; set; }

        [StringLength(500, ErrorMessage = "Corrective action cannot exceed 500 characters.")]
        public string? CorrectiveAction { get; set; }

        public Guid? InvolvedEmployeeId { get; set; }

        public Guid? SafetyInspectionId { get; set; }

        public IncidentStatus? Status { get; set; }

        [StringLength(1000, ErrorMessage = "Investigation notes cannot exceed 1000 characters.")]
        public string? InvestigationNotes { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Estimated cost must be non-negative.")]
        public int? EstimatedCost { get; set; }

        public bool? IsNearMiss { get; set; }

        [StringLength(500, ErrorMessage = "Witnesses cannot exceed 500 characters.")]
        public string? Witnesses { get; set; }
    }
} 