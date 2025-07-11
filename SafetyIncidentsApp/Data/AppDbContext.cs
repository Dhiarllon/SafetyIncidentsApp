using Microsoft.EntityFrameworkCore;
using SafetyIncidentsApp.Models;

namespace SafetyIncidentsApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Incident> Incidents { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<SafetyInspection> SafetyInspections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações do Incident
            modelBuilder.Entity<Incident>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.CorrectiveAction).HasMaxLength(500);
                entity.Property(e => e.InvestigationNotes).HasMaxLength(1000);
                entity.Property(e => e.Witnesses).HasMaxLength(500);
                entity.Property(e => e.EstimatedCost).HasDefaultValue(0);
                
                // Relacionamentos
                entity.HasOne(e => e.ReportedBy)
                    .WithMany(e => e.ReportedIncidents)
                    .HasForeignKey(e => e.ReportedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.InvolvedEmployee)
                    .WithMany(e => e.InvolvedIncidents)
                    .HasForeignKey(e => e.InvolvedEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.SafetyInspection)
                    .WithMany(e => e.RelatedIncidents)
                    .HasForeignKey(e => e.SafetyInspectionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configurações do Employee
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Position).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SafetyTrainingLevel).HasMaxLength(50);
                
                entity.HasIndex(e => e.EmployeeCode).IsUnique();
            });

            // Configurações do SafetyInspection
            modelBuilder.Entity<SafetyInspection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
                entity.Property(e => e.InspectorName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Findings).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Recommendations).HasMaxLength(1000);
                entity.Property(e => e.RiskScore).HasDefaultValue(0);
                
                // Relacionamento
                entity.HasOne(e => e.Inspector)
                    .WithMany(e => e.ConductedInspections)
                    .HasForeignKey(e => e.InspectorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data para testes
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Employees
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
                    LastSafetyTraining = DateTime.Now.AddMonths(-3)
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
                    LastSafetyTraining = DateTime.Now.AddMonths(-1)
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
                    LastSafetyTraining = DateTime.Now.AddMonths(-2)
                }
            };

            modelBuilder.Entity<Employee>().HasData(employees);

            // Seed SafetyInspections
            var inspections = new[]
            {
                new SafetyInspection
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    InspectionDate = DateTime.Now.AddDays(-5),
                    Location = "Andar 3 - Ala Norte",
                    InspectorName = "Maria Santos",
                    InspectorId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Type = InspectionType.Routine,
                    Status = InspectionStatus.Completed,
                    RiskScore = 3,
                    Findings = "Equipamentos de proteção em bom estado",
                    Recommendations = "Manter rotina de verificação semanal"
                }
            };

            modelBuilder.Entity<SafetyInspection>().HasData(inspections);
        }
    }
}
