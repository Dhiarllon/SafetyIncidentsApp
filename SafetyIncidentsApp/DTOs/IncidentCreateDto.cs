using System.ComponentModel.DataAnnotations;
using SafetyIncidentsApp.Models;

namespace SafetyIncidentsApp.DTOs
{
    public class IncidentCreateDto
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public IncidentType Type { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public SeverityLevel Severity { get; set; }

        public string? CorrectiveAction { get; set; }

        [Required]
        public Guid ReportedById { get; set; }

        public Guid? InvolvedEmployeeId { get; set; }

        public int EstimatedCost { get; set; } = 0;
    }
}
