using System.ComponentModel.DataAnnotations;
using SafetyIncidentsApp.Models;

namespace SafetyIncidentsApp.DTOs
{
    public class IncidentUpdateDto
    {
        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public IncidentType? Type { get; set; }

        public SeverityLevel? Severity { get; set; }

        [StringLength(500)]
        public string? CorrectiveAction { get; set; }

        public Guid? InvolvedEmployeeId { get; set; }

        public IncidentStatus? Status { get; set; }

        [Range(0, int.MaxValue)]
        public int? EstimatedCost { get; set; }
    }
} 