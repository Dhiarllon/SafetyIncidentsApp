using SafetyIncidentsApp.DTOs;

namespace SafetyIncidentsApp.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeReadDto>> GetAllAsync();
        Task<EmployeeReadDto?> GetByIdAsync(Guid id);
        Task<EmployeeReadDto> CreateAsync(EmployeeCreateDto employeeDto);
        Task<IEnumerable<EmployeeReadDto>> GetByDepartmentAsync(string department);
        Task<IEnumerable<EmployeeReadDto>> GetEmployeesNeedingTrainingAsync();
        Task UpdateTrainingRecordAsync(Guid id, DateTime trainingDate);
    }
} 