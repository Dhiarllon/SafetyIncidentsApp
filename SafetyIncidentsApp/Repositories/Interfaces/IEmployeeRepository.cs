using SafetyIncidentsApp.Models;

namespace SafetyIncidentsApp.Repositories.Interfaces
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        Task<IEnumerable<Employee>> GetActiveEmployeesAsync();
        Task<Employee?> GetByEmployeeCodeAsync(string employeeCode);
        Task<IEnumerable<Employee>> GetByDepartmentAsync(string department);
        Task<IEnumerable<Employee>> GetEmployeesNeedingTrainingAsync();
        Task<bool> EmployeeCodeExistsAsync(string employeeCode);
    }
} 