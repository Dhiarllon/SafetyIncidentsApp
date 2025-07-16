using Moq;
using AutoMapper;
using FluentAssertions;
using SafetyIncidentsApp.Services;
using SafetyIncidentsApp.Services.Interfaces;
using SafetyIncidentsApp.Repositories.Interfaces;
using SafetyIncidentsApp.DTOs;
using SafetyIncidentsApp.Models;
using SafetyIncidentsApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Xunit;

namespace SafetyIncidentsApp.Tests.Unit.Services
{
    public class IncidentServiceTests : IDisposable
    {
        private readonly Mock<IIncidentRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly AppDbContext _context;
        private readonly IncidentService _service;

        public IncidentServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _mockRepository = new Mock<IIncidentRepository>();
            _mockMapper = new Mock<IMapper>();
            _service = new IncidentService(_mockRepository.Object, _mockMapper.Object, _context);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        [Fact]
        public async Task CreateAsync_WithHighSeverityFall_ShouldRequireManagerApprovalAndSafetyReview()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = new Employee { Id = employeeId, IsActive = true };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Andar 5 - Ala Sul",
                Description = "Queda de altura",
                Type = IncidentType.Fall,
                Severity = SeverityLevel.High,
                ReportedById = employeeId,
                CorrectiveAction = "Implementar proteção coletiva",
                EstimatedCost = 15000
            };

            var incident = new Incident 
            { 
                Id = Guid.NewGuid(),
                Severity = SeverityLevel.High,
                Type = IncidentType.Fall,
                EstimatedCost = 15000
            };
            var incidentReadDto = new IncidentReadDto { Id = incident.Id };

            _mockRepository.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Incident, bool>>>()))
                .ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<Incident>(incidentDto)).Returns(incident);
            _mockRepository.Setup(r => r.GetByIdWithDetailsAsync(incident.Id))
                .ReturnsAsync(incident);
            _mockMapper.Setup(m => m.Map<IncidentReadDto>(incident)).Returns(incidentReadDto);

            // Act
            var result = await _service.CreateAsync(incidentDto);

            // Assert
            result.Should().NotBeNull();
            incident.RequiresManagerApproval.Should().BeTrue();
            incident.RequiresSafetyReview.Should().BeTrue();
            incident.Status.Should().Be(IncidentStatus.PendingApproval);
            
            _mockRepository.Verify(r => r.AddAsync(incident), Times.Once);
            _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithElectricShock_ShouldRequireInvestigationNotes()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = new Employee { Id = employeeId, IsActive = true };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Subestação",
                Description = "Choque elétrico",
                Type = IncidentType.ElectricShock,
                Severity = SeverityLevel.Medium,
                ReportedById = employeeId
                // Missing InvestigationNotes - should cause validation error
            };

            var incident = new Incident 
            { 
                Id = Guid.NewGuid(),
                Severity = SeverityLevel.Medium,
                Type = IncidentType.ElectricShock
            };

            _mockRepository.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Incident, bool>>>()))
                .ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<Incident>(incidentDto)).Returns(incident);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(incidentDto));
            exception.Message.Should().Contain("require investigation notes");
        }

        [Fact]
        public async Task CreateAsync_WithPPEIncidentAndUntrainedEmployee_ShouldThrowException()
        {
            // Arrange
            var reportedById = Guid.NewGuid();
            var involvedEmployeeId = Guid.NewGuid();
            
            var reportedBy = new Employee { Id = reportedById, IsActive = true };
            var involvedEmployee = new Employee 
            { 
                Id = involvedEmployeeId, 
                IsActive = true, 
                LastSafetyTraining = DateTime.UtcNow.AddMonths(-8) // Mais de 6 meses
            };
            
            _context.Employees.AddRange(reportedBy, involvedEmployee);
            await _context.SaveChangesAsync();

            var incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Canteiro de obras",
                Description = "Não uso de capacete",
                Type = IncidentType.ImproperUseOfPPE,
                Severity = SeverityLevel.Medium,
                ReportedById = reportedById,
                InvolvedEmployeeId = involvedEmployeeId
            };

            _mockRepository.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Incident, bool>>>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(incidentDto));
            exception.Message.Should().Contain("must have recent safety training");
        }

        [Fact]
        public async Task CreateAsync_WithHighCost_ShouldRequireManagerApproval()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = new Employee { Id = employeeId, IsActive = true };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Oficina",
                Description = "Danos em equipamento",
                Type = IncidentType.Other,
                Severity = SeverityLevel.Low,
                ReportedById = employeeId,
                EstimatedCost = 7500
            };

            var incident = new Incident 
            { 
                Id = Guid.NewGuid(),
                Severity = SeverityLevel.Low,
                Type = IncidentType.Other,
                EstimatedCost = 7500
            };
            var incidentReadDto = new IncidentReadDto { Id = incident.Id };

            _mockRepository.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Incident, bool>>>()))
                .ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<Incident>(incidentDto)).Returns(incident);
            _mockRepository.Setup(r => r.GetByIdWithDetailsAsync(incident.Id))
                .ReturnsAsync(incident);
            _mockMapper.Setup(m => m.Map<IncidentReadDto>(incident)).Returns(incidentReadDto);

            // Act
            var result = await _service.CreateAsync(incidentDto);

            // Assert
            incident.RequiresManagerApproval.Should().BeTrue();
            incident.Status.Should().Be(IncidentStatus.PendingApproval);
        }

        [Fact]
        public async Task CloseIncident_WithoutApproval_ShouldThrowException()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var incident = new Incident
            {
                Id = incidentId,
                IsResolved = false,
                RequiresManagerApproval = true,
                Status = IncidentStatus.PendingApproval
            };

            _mockRepository.Setup(r => r.GetByIdAsync(incidentId))
                .ReturnsAsync(incident);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.CloseIncidentAsync(incidentId));
            exception.Message.Should().Contain("requires manager approval");
        }

        [Fact]
        public async Task ApproveIncident_NotPendingApproval_ShouldThrowException()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var incident = new Incident
            {
                Id = incidentId,
                RequiresManagerApproval = true,
                Status = IncidentStatus.Reported // Não está pendente
            };

            _mockRepository.Setup(r => r.GetByIdAsync(incidentId))
                .ReturnsAsync(incident);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.ApproveIncidentAsync(incidentId, "Manager"));
            exception.Message.Should().Contain("not pending approval");
        }

        [Fact]
        public async Task UpdateAsync_ResolvedIncident_ShouldThrowException()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var incident = new Incident
            {
                Id = incidentId,
                IsResolved = true
            };

            var updateDto = new IncidentUpdateDto
            {
                Description = "Nova descrição"
            };

            _mockRepository.Setup(r => r.GetByIdAsync(incidentId))
                .ReturnsAsync(incident);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(incidentId, updateDto));
            exception.Message.Should().Contain("Cannot update a resolved incident");
        }

        [Theory]
        [InlineData(SeverityLevel.High, true, true, IncidentStatus.PendingApproval)]
        [InlineData(SeverityLevel.Medium, true, false, IncidentStatus.PendingApproval)]
        [InlineData(SeverityLevel.Low, false, false, IncidentStatus.Reported)]
        public async Task CreateAsync_WithDifferentSeverities_ShouldApplyCorrectBusinessRules(
            SeverityLevel severity, 
            bool expectedManagerApproval, 
            bool expectedSafetyReview, 
            IncidentStatus expectedStatus)
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = new Employee { Id = employeeId, IsActive = true };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test location",
                Description = "Test description",
                Type = IncidentType.Fall,
                Severity = severity,
                ReportedById = employeeId,
                EstimatedCost = 1000,
                CorrectiveAction = "Test corrective action" // Required for high severity fall
            };

            var incident = new Incident 
            { 
                Id = Guid.NewGuid(),
                Severity = severity,
                Type = IncidentType.Fall,
                EstimatedCost = 1000
            };
            var incidentReadDto = new IncidentReadDto { Id = incident.Id };

            _mockRepository.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Incident, bool>>>()))
                .ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<Incident>(incidentDto)).Returns(incident);
            _mockRepository.Setup(r => r.GetByIdWithDetailsAsync(incident.Id))
                .ReturnsAsync(incident);
            _mockMapper.Setup(m => m.Map<IncidentReadDto>(incident)).Returns(incidentReadDto);

            // Act
            var result = await _service.CreateAsync(incidentDto);

            // Assert
            incident.RequiresManagerApproval.Should().Be(expectedManagerApproval);
            incident.RequiresSafetyReview.Should().Be(expectedSafetyReview);
            incident.Status.Should().Be(expectedStatus);
        }

        [Fact]
        public async Task GetHighRiskIncidents_ShouldReturnOnlyHighRiskIncidents()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = new Employee { Id = employeeId, IsActive = true };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var highRiskIncidents = new List<Incident>
            {
                new Incident { Id = Guid.NewGuid(), Severity = SeverityLevel.High, ReportedById = employeeId },
                new Incident { Id = Guid.NewGuid(), EstimatedCost = 15000, ReportedById = employeeId }
            };

            var lowRiskIncidents = new List<Incident>
            {
                new Incident { Id = Guid.NewGuid(), Severity = SeverityLevel.Low, EstimatedCost = 1000, ReportedById = employeeId }
            };

            _context.Incidents.AddRange(highRiskIncidents);
            _context.Incidents.AddRange(lowRiskIncidents);
            await _context.SaveChangesAsync();

            var expectedDtos = highRiskIncidents.Select(i => new IncidentReadDto { Id = i.Id }).ToList();
            _mockMapper.Setup(m => m.Map<IEnumerable<IncidentReadDto>>(It.IsAny<IEnumerable<Incident>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _service.GetHighRiskIncidentsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }
    }
} 