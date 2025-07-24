using Microsoft.AspNetCore.Mvc;
using SafetyIncidentsApp.DTOs;
using SafetyIncidentsApp.Models;
using SafetyIncidentsApp.Services.Interfaces;

namespace SafetyIncidentsApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentController : ControllerBase
{
    private readonly IIncidentService _incidentService;

    public IncidentController(IIncidentService incidentService)
    {
        _incidentService = incidentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<IncidentReadDto>>> GetIncidents()
    {
        var incidents = await _incidentService.GetAllAsync();
        return Ok(incidents);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IncidentReadDto>> GetIncident(Guid id)
    {
        var incident = await _incidentService.GetByIdAsync(id);
        if (incident == null)
            return NotFound();

        return Ok(incident);
    }

    [HttpPost]
    public async Task<ActionResult<IncidentReadDto>> CreateIncident(IncidentCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _incidentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetIncident), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<IncidentReadDto>> UpdateIncident(Guid id, IncidentUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _incidentService.UpdateAsync(id, dto);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("by-severity")]
    public async Task<ActionResult<IEnumerable<IncidentReadDto>>> GetIncidentsBySeverity([FromQuery] SeverityLevel severity)
    {
        var result = await _incidentService.GetBySeverityAsync(severity);
        return Ok(result);
    }

    [HttpGet("pending-approval")]
    public async Task<ActionResult<IEnumerable<IncidentReadDto>>> GetPendingApprovalIncidents()
    {
        var result = await _incidentService.GetPendingApprovalAsync();
        return Ok(result);
    }

    [HttpPut("{id}/approve")]
    public async Task<IActionResult> ApproveIncident(Guid id, [FromBody] string approvedBy)
    {
        try
        {
            await _incidentService.ApproveIncidentAsync(id, approvedBy);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/close")]
    public async Task<IActionResult> CloseIncident(Guid id)
    {
        try
        {
            await _incidentService.CloseIncidentAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
