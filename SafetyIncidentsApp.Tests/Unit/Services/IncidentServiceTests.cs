using Moq;
using AutoMapper;
using FluentAssertions;
using SafetyIncidentsApp.Services;
using SafetyIncidentsApp.Repositories.Interfaces;
using SafetyIncidentsApp.DTOs;
using SafetyIncidentsApp.Models;
using SafetyIncidentsApp.Mappings;

namespace SafetyIncidentsApp.Tests.Unit.Services
{
    public class IncidentServiceTests
    {
        private readonly Mock<IIncidentRepository> _mockRepository;
        private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
        private readonly IMapper _mapper;
        private readonly IncidentService _service;

        public IncidentServiceTests()
        {
            _mockRepository = new Mock<IIncidentRepository>();
            _mockEmployeeRepository = new Mock<IEmployeeRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
            _service = new IncidentService(_mockRepository.Object, _mockEmployeeRepository.Object, _mapper);
        }

        [Fact]
        public async Task CreateAsync_WithHighSeverity_ShouldRequireManagerApproval()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = new Employee { Id = employeeId, IsActive = true };
            Incident? storedIncident = null;

            var incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Andar 5 - Ala Sul",
                Description = "Queda de altura",
                Type = IncidentType.Fall,
                Severity = SeverityLevel.High,
                ReportedById = employeeId,
                EstimatedCost = 15000
            };

            _mockEmployeeRepository.Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync(employee);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Incident>()))
                .Callback<Incident>(i => storedIncident = i)
                .Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => storedIncident);

            // Act
            var result = await _service.CreateAsync(incidentDto);

            // Assert
            result.Should().NotBeNull();
            storedIncident.Should().NotBeNull();
            storedIncident!.RequiresManagerApproval.Should().BeTrue();
            storedIncident.Status.Should().Be(IncidentStatus.PendingApproval);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Incident>()), Times.Once);
            _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithLowSeverity_ShouldNotRequireManagerApproval()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = new Employee { Id = employeeId, IsActive = true };
            Incident? storedIncident = null;

            var incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Andar 2 - Ala Norte",
                Description = "Escorregou no piso molhado",
                Type = IncidentType.Fall,
                Severity = SeverityLevel.Low,
                ReportedById = employeeId,
                EstimatedCost = 500
            };

            _mockEmployeeRepository.Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync(employee);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Incident>()))
                .Callback<Incident>(i => storedIncident = i)
                .Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => storedIncident);

            // Act
            var result = await _service.CreateAsync(incidentDto);

            // Assert
            result.Should().NotBeNull();
            storedIncident.Should().NotBeNull();
            storedIncident!.RequiresManagerApproval.Should().BeFalse();
            storedIncident.Status.Should().Be(IncidentStatus.Reported);
        }

        [Fact]
        public async Task CreateAsync_WithNonExistentEmployee_ShouldThrowException()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test Location",
                Description = "Test Description",
                Type = IncidentType.Fall,
                Severity = SeverityLevel.Low,
                ReportedById = employeeId // Non-existent employee
            };

            _mockEmployeeRepository.Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync((Employee?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(incidentDto));
            exception.Message.Should().Contain("Reported by employee not found");
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
                Description = "Updated description"
            };

            _mockRepository.Setup(r => r.GetByIdAsync(incidentId))
                .ReturnsAsync(incident);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(incidentId, updateDto));
            exception.Message.Should().Contain("Cannot update a resolved incident");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllIncidents()
        {
            // Arrange
            var incidents = new List<Incident>
            {
                new Incident { Id = Guid.NewGuid(), Description = "Incidente 1" },
                new Incident { Id = Guid.NewGuid(), Description = "Incidente 2" }
            };
            _mockRepository.Setup(r => r.GetAllWithDetailsAsync()).ReturnsAsync(incidents);

            // Act
            var result = (await _service.GetAllAsync()).ToList();

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(_mapper.Map<List<IncidentReadDto>>(incidents));
            result.Should().Contain(r => r.Description == "Incidente 1");
        }
    }
} 