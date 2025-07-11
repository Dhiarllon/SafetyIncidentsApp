using Microsoft.AspNetCore.Mvc;
using SafetyIncidentsApp.DTOs;
using SafetyIncidentsApp.Services.Interfaces;

namespace SafetyIncidentsApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeReadDto>>> GetEmployees()
    {
        var employees = await _employeeService.GetAllAsync();
        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeReadDto>> GetEmployee(Guid id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null)
            return NotFound();

        return Ok(employee);
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeReadDto>> CreateEmployee(EmployeeCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _employeeService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetEmployee), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("by-department")]
    public async Task<ActionResult<IEnumerable<EmployeeReadDto>>> GetEmployeesByDepartment([FromQuery] string department)
    {
        var result = await _employeeService.GetByDepartmentAsync(department);
        return Ok(result);
    }

    [HttpGet("needs-training")]
    public async Task<ActionResult<IEnumerable<EmployeeReadDto>>> GetEmployeesNeedingTraining()
    {
        var result = await _employeeService.GetEmployeesNeedingTrainingAsync();
        return Ok(result);
    }

    [HttpPut("{id}/update-training")]
    public async Task<IActionResult> UpdateTrainingRecord(Guid id, [FromBody] DateTime trainingDate)
    {
        try
        {
            await _employeeService.UpdateTrainingRecordAsync(id, trainingDate);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
} 