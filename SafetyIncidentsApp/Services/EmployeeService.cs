using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SafetyIncidentsApp.Data;
using SafetyIncidentsApp.DTOs;
using SafetyIncidentsApp.Models;
using SafetyIncidentsApp.Services.Interfaces;

namespace SafetyIncidentsApp.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public EmployeeService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EmployeeReadDto>> GetAllAsync()
    {
        var employees = await _context.Employees
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<EmployeeReadDto>>(employees);
    }

    public async Task<EmployeeReadDto?> GetByIdAsync(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        return employee == null ? null : _mapper.Map<EmployeeReadDto>(employee);
    }

    public async Task<EmployeeReadDto> CreateAsync(EmployeeCreateDto employeeDto)
    {
        await ValidateEmployeeCreation(employeeDto);

        var employee = _mapper.Map<Employee>(employeeDto);
        
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return _mapper.Map<EmployeeReadDto>(employee);
    }

    public async Task<IEnumerable<EmployeeReadDto>> GetByDepartmentAsync(string department)
    {
        var employees = await _context.Employees
            .Where(e => e.Department.Contains(department) && e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<EmployeeReadDto>>(employees);
    }

    public async Task<IEnumerable<EmployeeReadDto>> GetEmployeesNeedingTrainingAsync()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        
        var employees = await _context.Employees
            .Where(e => e.IsActive && 
                       (e.LastSafetyTraining == null || e.LastSafetyTraining < sixMonthsAgo))
            .OrderBy(e => e.LastSafetyTraining)
            .ToListAsync();

        return _mapper.Map<IEnumerable<EmployeeReadDto>>(employees);
    }

    public async Task UpdateTrainingRecordAsync(Guid id, DateTime trainingDate)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            throw new ArgumentException("Employee not found.");

        if (!employee.IsActive)
            throw new ArgumentException("Cannot update training record for inactive employee.");

        if (trainingDate > DateTime.UtcNow)
            throw new ArgumentException("Training date cannot be in the future.");

        if (trainingDate < DateTime.UtcNow.AddYears(-5))
            throw new ArgumentException("Training date cannot be more than 5 years in the past.");

        employee.LastSafetyTraining = trainingDate;
        await _context.SaveChangesAsync();
    }

    private async Task ValidateEmployeeCreation(EmployeeCreateDto employeeDto)
    {
        // Verificar se o código do funcionário já existe
        var existingEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeCode == employeeDto.EmployeeCode);

        if (existingEmployee != null)
            throw new ArgumentException("Employee code already exists.");

        // Verificar se a data de contratação é válida
        if (employeeDto.HireDate > DateTime.UtcNow)
            throw new ArgumentException("Hire date cannot be in the future.");

        if (employeeDto.HireDate < DateTime.UtcNow.AddYears(-50))
            throw new ArgumentException("Hire date cannot be more than 50 years in the past.");

        // Verificar se o último treinamento é válido (se fornecido)
        if (employeeDto.LastSafetyTraining.HasValue)
        {
            if (employeeDto.LastSafetyTraining.Value > DateTime.UtcNow)
                throw new ArgumentException("Last safety training date cannot be in the future.");

            if (employeeDto.LastSafetyTraining.Value < employeeDto.HireDate)
                throw new ArgumentException("Last safety training date cannot be before hire date.");
        }

        // Validar nível de treinamento
        if (!string.IsNullOrEmpty(employeeDto.SafetyTrainingLevel))
        {
            var validLevels = new[] { "Básico", "Intermediário", "Avançado", "Especialista" };
            if (!validLevels.Contains(employeeDto.SafetyTrainingLevel))
                throw new ArgumentException("Invalid safety training level.");
        }
    }
} 