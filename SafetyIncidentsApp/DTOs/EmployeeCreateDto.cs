using System.ComponentModel.DataAnnotations;

namespace SafetyIncidentsApp.DTOs
{
    public class EmployeeCreateDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Employee code is required.")]
        [StringLength(20, ErrorMessage = "Employee code cannot exceed 20 characters.")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters.")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Position is required.")]
        [StringLength(100, ErrorMessage = "Position cannot exceed 100 characters.")]
        public string Position { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hire date is required.")]
        public DateTime HireDate { get; set; }

        [StringLength(50, ErrorMessage = "Safety training level cannot exceed 50 characters.")]
        public string? SafetyTrainingLevel { get; set; }

        public DateTime? LastSafetyTraining { get; set; }
    }
} 