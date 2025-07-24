using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SafetyIncidentsApp.Data;
using SafetyIncidentsApp.Models;
using Microsoft.Data.Sqlite;

namespace SafetyIncidentsApp.Tests
{
    public static class TestConfiguration
    {
        private static readonly Dictionary<string, SqliteConnection> _connections = new();
        public static void ConfigureTestServices(IServiceCollection services, string databaseName = null)
        {
            // Remove all existing DbContext registrations
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType.Name.Contains("DbContextOptions") ||
                d.ServiceType == typeof(AppDbContext)).ToList();
            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Use a persistent SQLite in-memory connection for each databaseName
            if (string.IsNullOrEmpty(databaseName))
                databaseName = Guid.NewGuid().ToString();
            if (!_connections.ContainsKey(databaseName))
            {
                var connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
                connection.Open();
                _connections[databaseName] = connection;
            }
            var sharedConnection = _connections[databaseName];
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(sharedConnection);
            });
        }

        public static void InitializeTestDatabase(AppDbContext context)
        {
            // Ensure database is created
            context.Database.EnsureCreated();
            
            // Clear existing data
            context.Employees.RemoveRange(context.Employees);
            context.Incidents.RemoveRange(context.Incidents);
            context.SaveChanges();
            
            // Add seed data
            var employees = new[]
            {
                new Employee
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "João Silva",
                    EmployeeCode = "EMP001",
                    Department = "Construction",
                    Position = "Carpenter",
                    HireDate = DateTime.Now.AddYears(-2),
                    SafetyTrainingLevel = "Basic",
                    LastSafetyTraining = DateTime.Now.AddMonths(-3),
                    IsActive = true
                },
                new Employee
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Maria Santos",
                    EmployeeCode = "EMP002",
                    Department = "Safety",
                    Position = "Safety Technician",
                    HireDate = DateTime.Now.AddYears(-1),
                    SafetyTrainingLevel = "Advanced",
                    LastSafetyTraining = DateTime.Now.AddMonths(-8), // More than 6 months ago
                    IsActive = true
                },
                new Employee
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Pedro Costa",
                    EmployeeCode = "EMP003",
                    Department = "Construction",
                    Position = "Supervisor",
                    HireDate = DateTime.Now.AddYears(-3),
                    SafetyTrainingLevel = "Intermediate",
                    LastSafetyTraining = DateTime.Now.AddMonths(-2),
                    IsActive = true
                }
            };

            context.Employees.AddRange(employees);
            context.SaveChanges();
        }
    }
} 