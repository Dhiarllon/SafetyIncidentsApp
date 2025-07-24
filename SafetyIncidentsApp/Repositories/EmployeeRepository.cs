using Microsoft.EntityFrameworkCore;
using SafetyIncidentsApp.Data;
using SafetyIncidentsApp.Models;
using SafetyIncidentsApp.Repositories.Interfaces;

namespace SafetyIncidentsApp.Repositories
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
        {
            return await _context.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<Employee?> GetByEmployeeCodeAsync(string employeeCode)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentAsync(string department)
        {
            return await _context.Employees
                .Where(e => e.Department.Contains(department) && e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesNeedingTrainingAsync()
        {
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            
            return await _context.Employees
                .Where(e => e.IsActive && 
                           (e.LastSafetyTraining == null || e.LastSafetyTraining < sixMonthsAgo))
                .OrderBy(e => e.LastSafetyTraining)
                .ToListAsync();
        }

        public async Task<bool> EmployeeCodeExistsAsync(string employeeCode)
        {
            return await _context.Employees
                .AnyAsync(e => e.EmployeeCode == employeeCode);
        }
    }
} 