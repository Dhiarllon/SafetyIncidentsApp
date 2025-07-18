﻿using System.ComponentModel.DataAnnotations;
using SafetyIncidentsApp.Models;

namespace SafetyIncidentsApp.DTOs
{
    public class IncidentCreateDto
    {
        [Required(ErrorMessage = "Date is required.")]
        [CustomValidation(typeof(IncidentCreateDto), nameof(ValidateDate))]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required.")]
        [CustomValidation(typeof(IncidentCreateDto), nameof(ValidateType))]
        public IncidentType Type { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Severity is required.")]
        [CustomValidation(typeof(IncidentCreateDto), nameof(ValidateSeverity))]
        public SeverityLevel Severity { get; set; }

        [StringLength(500, ErrorMessage = "Corrective action cannot exceed 500 characters.")]
        public string? CorrectiveAction { get; set; }

        [Required(ErrorMessage = "ReportedById is required.")]
        [CustomValidation(typeof(IncidentCreateDto), nameof(ValidateReportedById))]
        public Guid ReportedById { get; set; }

        public Guid? InvolvedEmployeeId { get; set; }

        public Guid? SafetyInspectionId { get; set; }

        [StringLength(1000, ErrorMessage = "Investigation notes cannot exceed 1000 characters.")]
        public string? InvestigationNotes { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Estimated cost must be non-negative.")]
        public int EstimatedCost { get; set; } = 0;

        public bool IsNearMiss { get; set; } = false;

        [StringLength(500, ErrorMessage = "Witnesses cannot exceed 500 characters.")]
        public string? Witnesses { get; set; }

        public static ValidationResult? ValidateDate(DateTime date, ValidationContext context)
        {
            if (date > DateTime.UtcNow)
            {
                return new ValidationResult("Date cannot be in the future.", new[] { "Date" });
            }
            return ValidationResult.Success;
        }

        public static ValidationResult? ValidateType(IncidentType type, ValidationContext context)
        {
            if (!Enum.IsDefined(typeof(IncidentType), type))
            {
                return new ValidationResult("Type is invalid.", new[] { "Type" });
            }
            return ValidationResult.Success;
        }

        public static ValidationResult? ValidateSeverity(SeverityLevel severity, ValidationContext context)
        {
            if (!Enum.IsDefined(typeof(SeverityLevel), severity))
            {
                return new ValidationResult("Severity is invalid.", new[] { "Severity" });
            }
            return ValidationResult.Success;
        }

        public static ValidationResult? ValidateReportedById(Guid reportedById, ValidationContext context)
        {
            if (reportedById == Guid.Empty)
            {
                return new ValidationResult("ReportedById is required.", new[] { "ReportedById" });
            }
            return ValidationResult.Success;
        }
    }
}
