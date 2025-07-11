using Microsoft.EntityFrameworkCore;
using SafetyIncidentsApp.Data;
using SafetyIncidentsApp.Mappings;
using SafetyIncidentsApp.Repositories;
using SafetyIncidentsApp.Repositories.Interfaces;
using SafetyIncidentsApp.Services;
using SafetyIncidentsApp.Services.Interfaces;

namespace SafetyIncidentsApp;

public static class TestHostBuilder
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddControllers();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=incidents.db"));
        services.AddAutoMapper(typeof(MappingProfile));

        // Repositories
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Services
        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
    }
} 