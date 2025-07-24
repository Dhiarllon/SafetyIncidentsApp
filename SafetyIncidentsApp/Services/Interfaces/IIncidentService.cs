using SafetyIncidentsApp.DTOs;
using SafetyIncidentsApp.Models;

namespace SafetyIncidentsApp.Services.Interfaces
{
    public interface IIncidentService
    {
        Task<IEnumerable<IncidentReadDto>> GetAllAsync();
        Task<IncidentReadDto?> GetByIdAsync(Guid id);
        Task<IncidentReadDto> CreateAsync(IncidentCreateDto incidentDto);
        Task<IncidentReadDto> UpdateAsync(Guid id, IncidentUpdateDto updateDto);
        Task CloseIncidentAsync(Guid id);
        Task<IEnumerable<IncidentReadDto>> GetBySeverityAsync(SeverityLevel severity);
        Task<IEnumerable<IncidentReadDto>> GetPendingApprovalAsync();
        Task ApproveIncidentAsync(Guid id, string approvedBy);
    }
}
