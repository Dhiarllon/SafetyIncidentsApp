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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Incident configuration
            modelBuilder.Entity<Incident>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.CorrectiveAction).HasMaxLength(500);
                entity.Property(e => e.EstimatedCost).HasDefaultValue(0);
                
                // Relationships
                entity.HasOne(e => e.ReportedBy)
                    .WithMany(e => e.ReportedIncidents)
                    .HasForeignKey(e => e.ReportedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.InvolvedEmployee)
                    .WithMany(e => e.InvolvedIncidents)
                    .HasForeignKey(e => e.InvolvedEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Employee configuration
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

            // Seed data for tests
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
                    LastSafetyTraining = DateTime.Now.AddMonths(-1),
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

            modelBuilder.Entity<Employee>().HasData(employees);
        }
    }
}
