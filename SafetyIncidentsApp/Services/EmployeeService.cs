using AutoMapper;
using SafetyIncidentsApp.DTOs;
using SafetyIncidentsApp.Models;
using SafetyIncidentsApp.Repositories.Interfaces;
using SafetyIncidentsApp.Services.Interfaces;

namespace SafetyIncidentsApp.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IMapper _mapper;

    public EmployeeService(IEmployeeRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EmployeeReadDto>> GetAllAsync()
    {
        var employees = await _repository.GetActiveEmployeesAsync();
        return _mapper.Map<IEnumerable<EmployeeReadDto>>(employees);
    }

    public async Task<EmployeeReadDto?> GetByIdAsync(Guid id)
    {
        var employee = await _repository.GetByIdAsync(id);
        return employee == null ? null : _mapper.Map<EmployeeReadDto>(employee);
    }

    public async Task<EmployeeReadDto> CreateAsync(EmployeeCreateDto employeeDto)
    {
        await ValidateEmployeeCreation(employeeDto);

        var employee = _mapper.Map<Employee>(employeeDto);
        
        await _repository.AddAsync(employee);
        await _repository.SaveChangesAsync();

        return _mapper.Map<EmployeeReadDto>(employee);
    }

    public async Task<IEnumerable<EmployeeReadDto>> GetByDepartmentAsync(string department)
    {
        var employees = await _repository.GetByDepartmentAsync(department);
        return _mapper.Map<IEnumerable<EmployeeReadDto>>(employees);
    }

    public async Task<IEnumerable<EmployeeReadDto>> GetEmployeesNeedingTrainingAsync()
    {
        var employees = await _repository.GetEmployeesNeedingTrainingAsync();
        return _mapper.Map<IEnumerable<EmployeeReadDto>>(employees);
    }

    public async Task UpdateTrainingRecordAsync(Guid id, DateTime trainingDate)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null)
            throw new ArgumentException("Employee not found.");

        if (!employee.IsActive)
            throw new ArgumentException("Cannot update training record for inactive employee.");

        if (trainingDate > DateTime.UtcNow)
            throw new ArgumentException("Training date cannot be in the future.");

        if (trainingDate < DateTime.UtcNow.AddYears(-5))
            throw new ArgumentException("Training date cannot be more than 5 years in the past.");

        employee.LastSafetyTraining = trainingDate;
        await _repository.SaveChangesAsync();
    }

    private async Task ValidateEmployeeCreation(EmployeeCreateDto employeeDto)
    {
        // Verificar se o código do funcionário já existe
        var employeeCodeExists = await _repository.EmployeeCodeExistsAsync(employeeDto.EmployeeCode);

        if (employeeCodeExists)
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