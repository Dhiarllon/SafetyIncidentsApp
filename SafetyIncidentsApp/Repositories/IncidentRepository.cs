using Microsoft.EntityFrameworkCore;
using SafetyIncidentsApp.Data;
using SafetyIncidentsApp.Models;
using SafetyIncidentsApp.Repositories.Interfaces;

namespace SafetyIncidentsApp.Repositories
{
    public class IncidentRepository : GenericRepository<Incident>, IIncidentRepository
    {
        public IncidentRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Incident>> GetAllWithDetailsAsync()
        {
            return await _context.Incidents
                .Include(i => i.ReportedBy)
                .Include(i => i.InvolvedEmployee)
                .Include(i => i.SafetyInspection)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<Incident?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Incidents
                .Include(i => i.ReportedBy)
                .Include(i => i.InvolvedEmployee)
                .Include(i => i.SafetyInspection)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Incident>> GetRecentIncidentsAsync()
        {
            return await _context.Incidents
                .Include(i => i.ReportedBy)
                .Include(i => i.InvolvedEmployee)
                .OrderByDescending(i => i.Date)
                .Take(5)
                .ToListAsync();
        }

        public async Task<IEnumerable<Incident>> GetBySeverityAsync(SeverityLevel severity)
        {
            return await _context.Incidents
                .Include(i => i.ReportedBy)
                .Include(i => i.InvolvedEmployee)
                .Where(i => i.Severity == severity)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Incident>> GetByEmployeeAsync(Guid employeeId)
        {
            return await _context.Incidents
                .Include(i => i.ReportedBy)
                .Include(i => i.InvolvedEmployee)
                .Where(i => i.ReportedById == employeeId || i.InvolvedEmployeeId == employeeId)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Incident>> GetPendingApprovalAsync()
        {
            return await _context.Incidents
                .Include(i => i.ReportedBy)
                .Include(i => i.InvolvedEmployee)
                .Where(i => i.RequiresManagerApproval && i.Status == IncidentStatus.PendingApproval)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Incident>> GetHighRiskIncidentsAsync()
        {
            return await _context.Incidents
                .Include(i => i.ReportedBy)
                .Include(i => i.InvolvedEmployee)
                .Where(i => i.Severity == SeverityLevel.High || i.EstimatedCost > 10000)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Incident>> GetByLocationAsync(string location)
        {
            return await _context.Incidents
                .Include(i => i.ReportedBy)
                .Include(i => i.InvolvedEmployee)
                .Where(i => i.Location.Contains(location))
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Incident>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Incidents
                .Include(i => i.ReportedBy)
                .Include(i => i.InvolvedEmployee)
                .Where(i => i.Date >= startDate && i.Date <= endDate)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }
    }
}
