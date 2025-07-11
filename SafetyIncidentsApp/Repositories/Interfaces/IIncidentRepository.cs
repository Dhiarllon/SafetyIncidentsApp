using SafetyIncidentsApp.Models;

namespace SafetyIncidentsApp.Repositories.Interfaces
{
    public interface IIncidentRepository : IGenericRepository<Incident>
    {
        Task<IEnumerable<Incident>> GetAllWithDetailsAsync();
        Task<Incident?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<Incident>> GetRecentIncidentsAsync();
        Task<IEnumerable<Incident>> GetBySeverityAsync(SeverityLevel severity);
        Task<IEnumerable<Incident>> GetByEmployeeAsync(Guid employeeId);
        Task<IEnumerable<Incident>> GetPendingApprovalAsync();
        Task<IEnumerable<Incident>> GetHighRiskIncidentsAsync();
        Task<IEnumerable<Incident>> GetByLocationAsync(string location);
        Task<IEnumerable<Incident>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
