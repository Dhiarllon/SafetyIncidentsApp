using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using SafetyIncidentsApp.Controllers;
using SafetyIncidentsApp.DTOs;
using SafetyIncidentsApp.Models;
using SafetyIncidentsApp.Services.Interfaces;

namespace SafetyIncidentsApp.Tests.Unit.Controllers
{
    public class IncidentControllerTests
    {
        private readonly Mock<IIncidentService> _mockIncidentService;
        private readonly IncidentController _controller;

        public IncidentControllerTests()
        {
            _mockIncidentService = new Mock<IIncidentService>();
            _controller = new IncidentController(_mockIncidentService.Object);
        }

        [Fact]
        public async Task GetIncidents_ShouldReturnOkWithIncidents()
        {
            // Arrange
            var expectedIncidents = new List<IncidentReadDto>
            {
                new IncidentReadDto { Id = Guid.NewGuid(), Description = "Test 1" },
                new IncidentReadDto { Id = Guid.NewGuid(), Description = "Test 2" }
            };

            _mockIncidentService.Setup(s => s.GetAllAsync())
                .ReturnsAsync(expectedIncidents);

            // Act
            var result = await _controller.GetIncidents();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var incidents = okResult.Value.Should().BeOfType<List<IncidentReadDto>>().Subject;
            incidents.Should().HaveCount(2);
            incidents.Should().BeEquivalentTo(expectedIncidents);
        }

        [Fact]
        public async Task GetIncident_WithValidId_ShouldReturnOkWithIncident()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var expectedIncident = new IncidentReadDto 
            { 
                Id = incidentId, 
                Description = "Test Incident"
            };

            _mockIncidentService.Setup(s => s.GetByIdAsync(incidentId))
                .ReturnsAsync(expectedIncident);

            // Act
            var result = await _controller.GetIncident(incidentId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var incident = okResult.Value.Should().BeOfType<IncidentReadDto>().Subject;
            incident.Should().BeEquivalentTo(expectedIncident);
        }

        [Fact]
        public async Task GetIncident_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            _mockIncidentService.Setup(s => s.GetByIdAsync(incidentId))
                .ReturnsAsync((IncidentReadDto?)null);

            // Act
            var result = await _controller.GetIncident(incidentId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateIncident_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var createDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test Location",
                Description = "Test Description",
                Type = IncidentType.Fall,
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.NewGuid()
            };

            var createdIncident = new IncidentReadDto
            {
                Id = Guid.NewGuid(),
                Description = "Test Description"
            };

            _mockIncidentService.Setup(s => s.CreateAsync(createDto))
                .ReturnsAsync(createdIncident);

            // Act
            var result = await _controller.CreateIncident(createDto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var incident = createdResult.Value.Should().BeOfType<IncidentReadDto>().Subject;
            incident.Should().BeEquivalentTo(createdIncident);
            createdResult.ActionName.Should().Be(nameof(IncidentController.GetIncident));
            createdResult.RouteValues["id"].Should().Be(createdIncident.Id);
        }

        [Fact]
        public async Task CreateIncident_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var createDto = new IncidentCreateDto();
            _controller.ModelState.AddModelError("Description", "Description is required");

            // Act
            var result = await _controller.CreateIncident(createDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateIncident_WithServiceException_ShouldReturnBadRequest()
        {
            // Arrange
            var createDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test Location",
                Description = "Test Description",
                Type = IncidentType.Fall,
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.NewGuid()
            };

            _mockIncidentService.Setup(s => s.CreateAsync(createDto))
                .ThrowsAsync(new ArgumentException("Invalid employee"));

            // Act
            var result = await _controller.CreateIncident(createDto);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Invalid employee");
        }

        [Fact]
        public async Task UpdateIncident_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var updateDto = new IncidentUpdateDto
            {
                Description = "Updated Description",
                Location = "Updated Location"
            };

            var updatedIncident = new IncidentReadDto
            {
                Id = incidentId,
                Description = "Updated Description",
                Location = "Updated Location"
            };

            _mockIncidentService.Setup(s => s.UpdateAsync(incidentId, updateDto))
                .ReturnsAsync(updatedIncident);

            // Act
            var result = await _controller.UpdateIncident(incidentId, updateDto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var incident = okResult.Value.Should().BeOfType<IncidentReadDto>().Subject;
            incident.Should().BeEquivalentTo(updatedIncident);
        }

        [Fact]
        public async Task UpdateIncident_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var updateDto = new IncidentUpdateDto();
            _controller.ModelState.AddModelError("Description", "Description is required");

            // Act
            var result = await _controller.UpdateIncident(incidentId, updateDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateIncident_WithServiceException_ShouldReturnBadRequest()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var updateDto = new IncidentUpdateDto
            {
                Description = "Updated Description"
            };

            _mockIncidentService.Setup(s => s.UpdateAsync(incidentId, updateDto))
                .ThrowsAsync(new ArgumentException("Incident not found"));

            // Act
            var result = await _controller.UpdateIncident(incidentId, updateDto);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Incident not found");
        }

        [Fact]
        public async Task GetIncidentsBySeverity_ShouldReturnOkWithFilteredIncidents()
        {
            // Arrange
            var severity = SeverityLevel.High;
            var expectedIncidents = new List<IncidentReadDto>
            {
                new IncidentReadDto { Id = Guid.NewGuid(), Severity = SeverityLevel.High },
                new IncidentReadDto { Id = Guid.NewGuid(), Severity = SeverityLevel.High }
            };

            _mockIncidentService.Setup(s => s.GetBySeverityAsync(severity))
                .ReturnsAsync(expectedIncidents);

            // Act
            var result = await _controller.GetIncidentsBySeverity(severity);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var incidents = okResult.Value.Should().BeOfType<List<IncidentReadDto>>().Subject;
            incidents.Should().HaveCount(2);
            incidents.Should().BeEquivalentTo(expectedIncidents);
        }

        [Fact]
        public async Task GetPendingApprovalIncidents_ShouldReturnOkWithPendingIncidents()
        {
            // Arrange
            var expectedIncidents = new List<IncidentReadDto>
            {
                new IncidentReadDto { Id = Guid.NewGuid(), RequiresManagerApproval = true },
                new IncidentReadDto { Id = Guid.NewGuid(), RequiresManagerApproval = true }
            };

            _mockIncidentService.Setup(s => s.GetPendingApprovalAsync())
                .ReturnsAsync(expectedIncidents);

            // Act
            var result = await _controller.GetPendingApprovalIncidents();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var incidents = okResult.Value.Should().BeOfType<List<IncidentReadDto>>().Subject;
            incidents.Should().HaveCount(2);
            incidents.Should().BeEquivalentTo(expectedIncidents);
        }

        [Fact]
        public async Task ApproveIncident_WithValidData_ShouldReturnNoContent()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var approvedBy = "Manager Name";

            _mockIncidentService.Setup(s => s.ApproveIncidentAsync(incidentId, approvedBy))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ApproveIncident(incidentId, approvedBy);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task ApproveIncident_WithServiceException_ShouldReturnBadRequest()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var approvedBy = "Manager Name";

            _mockIncidentService.Setup(s => s.ApproveIncidentAsync(incidentId, approvedBy))
                .ThrowsAsync(new ArgumentException("Incident not found"));

            // Act
            var result = await _controller.ApproveIncident(incidentId, approvedBy);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Incident not found");
        }

        [Fact]
        public async Task CloseIncident_WithValidData_ShouldReturnNoContent()
        {
            // Arrange
            var incidentId = Guid.NewGuid();

            _mockIncidentService.Setup(s => s.CloseIncidentAsync(incidentId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CloseIncident(incidentId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task CloseIncident_WithServiceException_ShouldReturnBadRequest()
        {
            // Arrange
            var incidentId = Guid.NewGuid();

            _mockIncidentService.Setup(s => s.CloseIncidentAsync(incidentId))
                .ThrowsAsync(new ArgumentException("Incident requires manager approval"));

            // Act
            var result = await _controller.CloseIncident(incidentId);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Incident requires manager approval");
        }
    }
} 