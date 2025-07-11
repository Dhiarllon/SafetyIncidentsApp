using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SafetyIncidentsApp.Data;

namespace SafetyIncidentsApp.Tests
{
    public static class TestConfiguration
    {
        public static void ConfigureTestServices(IServiceCollection services, string databaseName = null)
        {
            // Remove all existing DbContext registrations
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            var dbContextServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(AppDbContext));
            if (dbContextServiceDescriptor != null)
            {
                services.Remove(dbContextServiceDescriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString());
            });
        }

        public static void InitializeTestDatabase(AppDbContext context)
        {
            // Ensure database is created
            context.Database.EnsureCreated();
            
            // Clear existing data
            context.Employees.RemoveRange(context.Employees);
            context.Incidents.RemoveRange(context.Incidents);
            context.SafetyInspections.RemoveRange(context.SafetyInspections);
            context.SaveChanges();
            
            // Add seed data
            var employees = new[]
            {
                new Employee
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "João Silva",
                    EmployeeCode = "EMP001",
                    Department = "Construção",
                    Position = "Pedreiro",
                    HireDate = DateTime.Now.AddYears(-2),
                    SafetyTrainingLevel = "Básico",
                    LastSafetyTraining = DateTime.Now.AddMonths(-3),
                    IsActive = true
                },
                new Employee
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Maria Santos",
                    EmployeeCode = "EMP002",
                    Department = "Segurança",
                    Position = "Técnico de Segurança",
                    HireDate = DateTime.Now.AddYears(-1),
                    SafetyTrainingLevel = "Avançado",
                    LastSafetyTraining = DateTime.Now.AddMonths(-1),
                    IsActive = true
                },
                new Employee
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Pedro Costa",
                    EmployeeCode = "EMP003",
                    Department = "Construção",
                    Position = "Encarregado",
                    HireDate = DateTime.Now.AddYears(-3),
                    SafetyTrainingLevel = "Intermediário",
                    LastSafetyTraining = DateTime.Now.AddMonths(-2),
                    IsActive = true
                }
            };

            context.Employees.AddRange(employees);
            context.SaveChanges();
        }
    }
} 