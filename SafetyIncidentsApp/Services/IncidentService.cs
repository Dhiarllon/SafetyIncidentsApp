using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SafetyIncidentsApp.Data;
using SafetyIncidentsApp.DTOs;
using SafetyIncidentsApp.Models;
using SafetyIncidentsApp.Repositories.Interfaces;
using SafetyIncidentsApp.Services.Interfaces;

namespace SafetyIncidentsApp.Services;

public class IncidentService : IIncidentService
{
    private readonly IIncidentRepository _repository;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public IncidentService(IIncidentRepository repository, IMapper mapper, AppDbContext context)
    {
        _repository = repository;
        _mapper = mapper;
        _context = context;
    }

    public async Task<IEnumerable<IncidentReadDto>> GetAllAsync()
    {
        var incidents = await _repository.GetAllWithDetailsAsync();
        return _mapper.Map<IEnumerable<IncidentReadDto>>(incidents);
    }

    public async Task<IncidentReadDto?> GetByIdAsync(Guid id)
    {
        var incident = await _repository.GetByIdWithDetailsAsync(id);
        return incident == null ? null : _mapper.Map<IncidentReadDto>(incident);
    }

    public async Task<IncidentReadDto> CreateAsync(IncidentCreateDto incidentDto)
    {
        await ValidateIncidentCreation(incidentDto);

        var incident = _mapper.Map<Incident>(incidentDto);
        
        // Aplicar regras de negócio baseadas na severidade
        ApplyBusinessRules(incident);

        await _repository.AddAsync(incident);
        await _repository.SaveChangesAsync();

        // Recarregar com relacionamentos para retornar dados completos
        var createdIncident = await _repository.GetByIdWithDetailsAsync(incident.Id);
        return _mapper.Map<IncidentReadDto>(createdIncident!);
    }

    public async Task<IncidentReadDto> UpdateAsync(Guid id, IncidentUpdateDto updateDto)
    {
        var incident = await _repository.GetByIdAsync(id);
        if (incident == null)
            throw new ArgumentException("Incident not found.");

        if (incident.IsResolved)
            throw new ArgumentException("Cannot update a resolved incident.");

        await ValidateIncidentUpdate(incident, updateDto);

        _mapper.Map(updateDto, incident);
        
        // Reaplicar regras de negócio se a severidade mudou
        if (updateDto.Severity.HasValue && updateDto.Severity.Value != incident.Severity)
        {
            ApplyBusinessRules(incident);
        }

        await _repository.SaveChangesAsync();

        var updatedIncident = await _repository.GetByIdWithDetailsAsync(incident.Id);
        return _mapper.Map<IncidentReadDto>(updatedIncident!);
    }

    public async Task CloseIncidentAsync(Guid id)
    {
        var incident = await _repository.GetByIdAsync(id);

        if (incident == null)
            throw new ArgumentException("Incident not found.");

        if (incident.IsResolved)
            throw new ArgumentException("Incident is already resolved.");

        // Verificar se pode ser fechado
        if (incident.RequiresManagerApproval && incident.Status != IncidentStatus.Approved)
            throw new ArgumentException("Incident requires manager approval before closing.");

        if (incident.RequiresSafetyReview && incident.Status != IncidentStatus.Approved)
            throw new ArgumentException("Incident requires safety review before closing.");

        incident.IsResolved = true;
        incident.ResolvedDate = DateTime.UtcNow;
        incident.Status = IncidentStatus.Closed;
        
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<IncidentReadDto>> GetRecentIncidentsAsync()
    {
        var result = await _repository.GetRecentIncidentsAsync();
        return _mapper.Map<IEnumerable<IncidentReadDto>>(result);
    }

    public async Task<IEnumerable<IncidentReadDto>> GetBySeverityAsync(SeverityLevel severity)
    {
        var result = await _repository.GetBySeverityAsync(severity);
        return _mapper.Map<IEnumerable<IncidentReadDto>>(result);
    }

    public async Task<IEnumerable<IncidentReadDto>> GetByEmployeeAsync(Guid employeeId)
    {
        var incidents = await _context.Incidents
            .Include(i => i.ReportedBy)
            .Include(i => i.InvolvedEmployee)
            .Where(i => i.ReportedById == employeeId || i.InvolvedEmployeeId == employeeId)
            .OrderByDescending(i => i.Date)
            .ToListAsync();

        return _mapper.Map<IEnumerable<IncidentReadDto>>(incidents);
    }

    public async Task<IEnumerable<IncidentReadDto>> GetPendingApprovalAsync()
    {
        var incidents = await _context.Incidents
            .Include(i => i.ReportedBy)
            .Include(i => i.InvolvedEmployee)
            .Where(i => i.RequiresManagerApproval && i.Status == IncidentStatus.PendingApproval)
            .OrderByDescending(i => i.Date)
            .ToListAsync();

        return _mapper.Map<IEnumerable<IncidentReadDto>>(incidents);
    }

    public async Task ApproveIncidentAsync(Guid id, string approvedBy)
    {
        var incident = await _repository.GetByIdAsync(id);
        if (incident == null)
            throw new ArgumentException("Incident not found.");

        if (!incident.RequiresManagerApproval)
            throw new ArgumentException("This incident does not require approval.");

        if (incident.Status != IncidentStatus.PendingApproval)
            throw new ArgumentException("Incident is not pending approval.");

        incident.Status = IncidentStatus.Approved;
        incident.ManagerApprovalDate = DateTime.UtcNow;
        incident.ManagerApprovedBy = approvedBy;

        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<IncidentReadDto>> GetHighRiskIncidentsAsync()
    {
        var incidents = await _context.Incidents
            .Include(i => i.ReportedBy)
            .Include(i => i.InvolvedEmployee)
            .Where(i => i.Severity == SeverityLevel.High || i.EstimatedCost > 10000)
            .OrderByDescending(i => i.Date)
            .ToListAsync();

        return _mapper.Map<IEnumerable<IncidentReadDto>>(incidents);
    }

    private async Task ValidateIncidentCreation(IncidentCreateDto incidentDto)
    {
        // Validações básicas
        if (incidentDto.Date > DateTime.UtcNow)
            throw new ArgumentException("Incident date cannot be in the future.");

        if (incidentDto.Date < DateTime.UtcNow.AddYears(-1))
            throw new ArgumentException("Incident date cannot be more than 1 year in the past.");

        // Verificar se o funcionário que reportou existe
        var reportedBy = await _context.Employees.FindAsync(incidentDto.ReportedById);
        if (reportedBy == null)
            throw new ArgumentException("Reported by employee not found.");

        if (!reportedBy.IsActive)
            throw new ArgumentException("Cannot report incident with inactive employee.");

        // Verificar se o funcionário envolvido existe (se fornecido)
        if (incidentDto.InvolvedEmployeeId.HasValue)
        {
            var involvedEmployee = await _context.Employees.FindAsync(incidentDto.InvolvedEmployeeId.Value);
            if (involvedEmployee == null)
                throw new ArgumentException("Involved employee not found.");

            if (!involvedEmployee.IsActive)
                throw new ArgumentException("Cannot involve inactive employee in incident.");
        }

        // Verificar se a inspeção de segurança existe (se fornecida)
        if (incidentDto.SafetyInspectionId.HasValue)
        {
            var inspection = await _context.SafetyInspections.FindAsync(incidentDto.SafetyInspectionId.Value);
            if (inspection == null)
                throw new ArgumentException("Safety inspection not found.");
        }

        // Verificar duplicatas
        var isDuplicate = await IsDuplicate(incidentDto);
        if (isDuplicate)
            throw new ArgumentException("Duplicate incident detected: same location, description, and date.");

        // Validações específicas por tipo de incidente
        await ValidateIncidentTypeSpecificRules(incidentDto);
    }

    private async Task ValidateIncidentUpdate(Incident incident, IncidentUpdateDto updateDto)
    {
        // Verificar se o funcionário envolvido existe (se fornecido)
        if (updateDto.InvolvedEmployeeId.HasValue)
        {
            var involvedEmployee = await _context.Employees.FindAsync(updateDto.InvolvedEmployeeId.Value);
            if (involvedEmployee == null)
                throw new ArgumentException("Involved employee not found.");

            if (!involvedEmployee.IsActive)
                throw new ArgumentException("Cannot involve inactive employee in incident.");
        }

        // Verificar se a inspeção de segurança existe (se fornecida)
        if (updateDto.SafetyInspectionId.HasValue)
        {
            var inspection = await _context.SafetyInspections.FindAsync(updateDto.SafetyInspectionId.Value);
            if (inspection == null)
                throw new ArgumentException("Safety inspection not found.");
        }
    }

    private async Task ValidateIncidentTypeSpecificRules(IncidentCreateDto incidentDto)
    {
        switch (incidentDto.Type)
        {
            case IncidentType.Fall:
                if (incidentDto.Severity == SeverityLevel.High)
                {
                    // Incidentes de queda de alta severidade sempre precisam de aprovação
                    if (string.IsNullOrEmpty(incidentDto.CorrectiveAction))
                        throw new ArgumentException("High severity fall incidents require corrective action.");
                }
                break;

            case IncidentType.ElectricShock:
                // Incidentes de choque elétrico sempre precisam de revisão de segurança
                if (incidentDto.Severity != SeverityLevel.Low)
                {
                    if (string.IsNullOrEmpty(incidentDto.InvestigationNotes))
                        throw new ArgumentException("Electric shock incidents require investigation notes.");
                }
                break;

            case IncidentType.ImproperUseOfPPE:
                // Verificar se o funcionário envolvido tem treinamento adequado
                if (incidentDto.InvolvedEmployeeId.HasValue)
                {
                    var employee = await _context.Employees.FindAsync(incidentDto.InvolvedEmployeeId.Value);
                    if (employee?.LastSafetyTraining == null || 
                        employee.LastSafetyTraining < DateTime.UtcNow.AddMonths(-6))
                    {
                        throw new ArgumentException("Employee involved in PPE incident must have recent safety training.");
                    }
                }
                break;
        }
    }

    private void ApplyBusinessRules(Incident incident)
    {
        // Regras baseadas na severidade
        switch (incident.Severity)
        {
            case SeverityLevel.High:
                incident.RequiresManagerApproval = true;
                incident.RequiresSafetyReview = true;
                incident.Status = IncidentStatus.PendingApproval;
                break;

            case SeverityLevel.Medium:
                incident.RequiresManagerApproval = true;
                incident.RequiresSafetyReview = false;
                incident.Status = IncidentStatus.PendingApproval;
                break;

            case SeverityLevel.Low:
                incident.RequiresManagerApproval = false;
                incident.RequiresSafetyReview = false;
                incident.Status = IncidentStatus.Reported;
                break;
        }

        // Regras baseadas no tipo
        switch (incident.Type)
        {
            case IncidentType.ElectricShock:
                incident.RequiresSafetyReview = true;
                break;

            case IncidentType.Fall when incident.Severity == SeverityLevel.High:
                incident.RequiresManagerApproval = true;
                incident.RequiresSafetyReview = true;
                break;
        }

        // Regras baseadas no custo estimado
        if (incident.EstimatedCost > 5000)
        {
            incident.RequiresManagerApproval = true;
        }

        if (incident.EstimatedCost > 10000)
        {
            incident.RequiresSafetyReview = true;
        }
    }

    private async Task<bool> IsDuplicate(IncidentCreateDto incidentDto)
    {
        return await _repository.AnyAsync(i =>
           i.Location == incidentDto.Location &&
           i.Description == incidentDto.Description &&
           i.Date.Date == incidentDto.Date.Date &&
           i.ReportedById == incidentDto.ReportedById);
    }
}
