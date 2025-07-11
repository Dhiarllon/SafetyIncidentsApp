namespace SafetyIncidentsApp.DTOs
{
    public class EmployeeReadDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? SafetyTrainingLevel { get; set; }
        public DateTime? LastSafetyTraining { get; set; }
    }
} 