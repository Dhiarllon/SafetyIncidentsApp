using AutoMapper;
using SafetyIncidentsApp.DTOs;
using SafetyIncidentsApp.Models;
using SafetyIncidentsApp.Repositories.Interfaces;
using SafetyIncidentsApp.Services.Interfaces;

namespace SafetyIncidentsApp.Services;

public class IncidentService : IIncidentService
{
    private readonly IIncidentRepository _repository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;

    public IncidentService(IIncidentRepository repository, IEmployeeRepository employeeRepository, IMapper mapper)
    {
        _repository = repository;
        _employeeRepository = employeeRepository;
        _mapper = mapper;
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

        if (incident.RequiresManagerApproval && incident.Status != IncidentStatus.Approved)
            throw new ArgumentException("Incident requires manager approval before closing.");

        incident.IsResolved = true;
        incident.ResolvedDate = DateTime.UtcNow;
        incident.Status = IncidentStatus.Closed;
        
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<IncidentReadDto>> GetBySeverityAsync(SeverityLevel severity)
    {
        var result = await _repository.GetBySeverityAsync(severity);
        return _mapper.Map<IEnumerable<IncidentReadDto>>(result);
    }

    public async Task<IEnumerable<IncidentReadDto>> GetPendingApprovalAsync()
    {
        var incidents = await _repository.GetPendingApprovalAsync();
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

        await _repository.SaveChangesAsync();
    }

    private async Task ValidateIncidentCreation(IncidentCreateDto incidentDto)
    {
        // Validações básicas
        if (incidentDto.Date > DateTime.UtcNow)
            throw new ArgumentException("Incident date cannot be in the future.");

        // Verificar se o funcionário que reportou existe
        var reportedBy = await _employeeRepository.GetByIdAsync(incidentDto.ReportedById);
        if (reportedBy == null)
            throw new ArgumentException("Reported by employee not found.");

        if (!reportedBy.IsActive)
            throw new ArgumentException("Cannot report incident with inactive employee.");

        // Verificar se o funcionário envolvido existe (se fornecido)
        if (incidentDto.InvolvedEmployeeId.HasValue)
        {
            var involvedEmployee = await _employeeRepository.GetByIdAsync(incidentDto.InvolvedEmployeeId.Value);
            if (involvedEmployee == null)
                throw new ArgumentException("Involved employee not found.");

            if (!involvedEmployee.IsActive)
                throw new ArgumentException("Cannot involve inactive employee in incident.");
        }
    }

    private void ApplyBusinessRules(Incident incident)
    {
        // Regras de negócio simplificadas
        switch (incident.Severity)
        {
            case SeverityLevel.High:
                incident.RequiresManagerApproval = true;
                incident.Status = IncidentStatus.PendingApproval;
                break;
            case SeverityLevel.Medium:
                if (incident.EstimatedCost > 5000)
                {
                    incident.RequiresManagerApproval = true;
                    incident.Status = IncidentStatus.PendingApproval;
                }
                break;
            case SeverityLevel.Low:
                incident.RequiresManagerApproval = false;
                incident.Status = IncidentStatus.Reported;
                break;
        }
    }
}
