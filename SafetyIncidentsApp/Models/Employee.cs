namespace SafetyIncidentsApp.Models
{
    public class Employee
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
        
        // Navigation properties
        public virtual ICollection<Incident> ReportedIncidents { get; set; } = new List<Incident>();
        public virtual ICollection<Incident> InvolvedIncidents { get; set; } = new List<Incident>();
    }
} 