using System.ComponentModel.DataAnnotations;
using SafetyIncidentsApp.Models;

namespace SafetyIncidentsApp.DTOs
{
    public class SafetyInspectionCreateDto
    {
        [Required(ErrorMessage = "Inspection date is required.")]
        public DateTime InspectionDate { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Inspector name is required.")]
        [StringLength(100, ErrorMessage = "Inspector name cannot exceed 100 characters.")]
        public string InspectorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Inspector ID is required.")]
        public Guid InspectorId { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        public InspectionType Type { get; set; }

        [Required(ErrorMessage = "Findings are required.")]
        [StringLength(1000, ErrorMessage = "Findings cannot exceed 1000 characters.")]
        public string Findings { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Recommendations cannot exceed 1000 characters.")]
        public string? Recommendations { get; set; }

        [Range(0, 10, ErrorMessage = "Risk score must be between 0 and 10.")]
        public int RiskScore { get; set; } = 0;

        public DateTime? FollowUpDate { get; set; }
        public bool RequiresFollowUp { get; set; } = false;
    }
} 