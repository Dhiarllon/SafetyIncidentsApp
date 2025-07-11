using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using SafetyIncidentsApp.Data;
using SafetyIncidentsApp.DTOs;
using SafetyIncidentsApp.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace SafetyIncidentsApp.Tests.Integration
{
    public class IncidentControllerIntegrationTests : IClassFixture<WebApplicationFactory<IApiMarker>>
    {
        private readonly WebApplicationFactory<IApiMarker> _factory;
        private readonly HttpClient _client;
        private readonly string _databaseName;

        public IncidentControllerIntegrationTests(WebApplicationFactory<IApiMarker> factory)
        {
            _databaseName = Guid.NewGuid().ToString();
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    TestConfiguration.ConfigureTestServices(services, _databaseName);
                });
            });

            _client = _factory.CreateClient();
            
            // Initialize database with seed data
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            TestConfiguration.InitializeTestDatabase(context);
        }

        [Fact]
        public async Task GetIncidents_ShouldReturnEmptyList_WhenNoIncidentsExist()
        {
            // Act
            var response = await _client.GetAsync("/api/incident");
            var content = await response.Content.ReadAsStringAsync();
            var incidents = JsonSerializer.Deserialize<List<IncidentReadDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            incidents.Should().NotBeNull();
            incidents.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateIncident_WithValidData_ShouldReturnCreatedIncident()
        {
            // Arrange
            var incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Andar 3 - Ala Norte",
                Type = IncidentType.Fall,
                Description = "Queda de altura",
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.Parse("11111111-1111-1111-1111-111111111111"), // From seed data
                CorrectiveAction = "Implementar proteção coletiva",
                EstimatedCost = 5000
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/incident", incidentDto);
            var content = await response.Content.ReadAsStringAsync();
            var createdIncident = JsonSerializer.Deserialize<IncidentReadDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            createdIncident.Should().NotBeNull();
            createdIncident.Id.Should().NotBe(Guid.Empty);
            createdIncident.Location.Should().Be(incidentDto.Location);
            createdIncident.Description.Should().Be(incidentDto.Description);
            createdIncident.RequiresManagerApproval.Should().BeTrue(); // Due to cost > 5000
            createdIncident.Status.Should().Be(IncidentStatus.PendingApproval);
        }

        [Fact]
        public async Task CreateIncident_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var incidentDto = new IncidentCreateDto
            {
                // Missing required fields
                Description = "Test description"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/incident", incidentDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateIncident_WithNonExistentEmployee_ShouldReturnBadRequest()
        {
            // Arrange
            var incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test location",
                Type = IncidentType.Fall,
                Description = "Test description",
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.NewGuid(), // Non-existent employee
                EstimatedCost = 1000
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/incident", incidentDto);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            content.Should().Contain("Reported by employee not found");
        }

        [Fact]
        public async Task CreateIncident_WithHighSeverityFall_ShouldRequireApproval()
        {
            // Arrange
            var incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Andar 10 - Ala Sul",
                Type = IncidentType.Fall,
                Description = "Queda de altura crítica",
                Severity = SeverityLevel.High,
                ReportedById = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CorrectiveAction = "Implementar proteção coletiva imediata",
                EstimatedCost = 15000
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/incident", incidentDto);
            var content = await response.Content.ReadAsStringAsync();
            var createdIncident = JsonSerializer.Deserialize<IncidentReadDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            createdIncident.Should().NotBeNull();
            createdIncident.RequiresManagerApproval.Should().BeTrue();
            createdIncident.RequiresSafetyReview.Should().BeTrue();
            createdIncident.Status.Should().Be(IncidentStatus.PendingApproval);
        }

        [Fact]
        public async Task GetIncident_WithValidId_ShouldReturnIncident()
        {
            // Arrange - Create an incident first
            var incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test location",
                Type = IncidentType.Fall,
                Description = "Test description",
                Severity = SeverityLevel.Low,
                ReportedById = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                EstimatedCost = 1000
            };

            var createResponse = await _client.PostAsJsonAsync("/api/incident", incidentDto);
            var createdIncident = JsonSerializer.Deserialize<IncidentReadDto>(
                await createResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Act
            var response = await _client.GetAsync($"/api/incident/{createdIncident.Id}");
            var content = await response.Content.ReadAsStringAsync();
            var incident = JsonSerializer.Deserialize<IncidentReadDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            incident.Should().NotBeNull();
            incident.Id.Should().Be(createdIncident.Id);
            incident.Location.Should().Be(incidentDto.Location);
        }

        [Fact]
        public async Task GetIncident_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/incident/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateIncident_WithValidData_ShouldReturnUpdatedIncident()
        {
            // Arrange - Create an incident first
            var incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Original location",
                Type = IncidentType.Fall,
                Description = "Original description",
                Severity = SeverityLevel.Low,
                ReportedById = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                EstimatedCost = 1000
            };

            var createResponse = await _client.PostAsJsonAsync("/api/incident", incidentDto);
            var createdIncident = JsonSerializer.Deserialize<IncidentReadDto>(
                await createResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var updateDto = new IncidentUpdateDto
            {
                Description = "Updated description",
                Location = "Updated location"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/incident/{createdIncident.Id}", updateDto);
            var content = await response.Content.ReadAsStringAsync();
            var updatedIncident = JsonSerializer.Deserialize<IncidentReadDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            updatedIncident.Should().NotBeNull();
            updatedIncident.Description.Should().Be(updateDto.Description);
            updatedIncident.Location.Should().Be(updateDto.Location);
        }

        [Fact]
        public async Task ApproveIncident_WithValidApproval_ShouldSucceed()
        {
            // Arrange - Create an incident that requires approval
            var incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test location",
                Type = IncidentType.Fall,
                Description = "Test description",
                Severity = SeverityLevel.High,
                ReportedById = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                EstimatedCost = 15000
            };

            var createResponse = await _client.PostAsJsonAsync("/api/incident", incidentDto);
            var createdIncident = JsonSerializer.Deserialize<IncidentReadDto>(
                await createResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Act
            var response = await _client.PostAsync($"/api/incident/{createdIncident.Id}/approve?approvedBy=Manager", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CloseIncident_WithoutApproval_ShouldReturnBadRequest()
        {
            // Arrange - Create an incident that requires approval
            var incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test location",
                Type = IncidentType.Fall,
                Description = "Test description",
                Severity = SeverityLevel.High,
                ReportedById = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                EstimatedCost = 15000
            };

            var createResponse = await _client.PostAsJsonAsync("/api/incident", incidentDto);
            var createdIncident = JsonSerializer.Deserialize<IncidentReadDto>(
                await createResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Act
            var response = await _client.PostAsync($"/api/incident/{createdIncident.Id}/close", null);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            content.Should().Contain("requires manager approval");
        }

        [Fact]
        public async Task GetIncidentsBySeverity_ShouldReturnFilteredResults()
        {
            // Arrange - Create incidents with different severities
            var highSeverityDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "High severity location",
                Type = IncidentType.Fall,
                Description = "High severity incident",
                Severity = SeverityLevel.High,
                ReportedById = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                EstimatedCost = 1000
            };

            var lowSeverityDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Low severity location",
                Type = IncidentType.Fall,
                Description = "Low severity incident",
                Severity = SeverityLevel.Low,
                ReportedById = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                EstimatedCost = 1000
            };

            await _client.PostAsJsonAsync("/api/incident", highSeverityDto);
            await _client.PostAsJsonAsync("/api/incident", lowSeverityDto);

            // Act
            var response = await _client.GetAsync("/api/incident/severity/High");
            var content = await response.Content.ReadAsStringAsync();
            var incidents = JsonSerializer.Deserialize<List<IncidentReadDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            incidents.Should().NotBeNull();
            incidents.Should().OnlyContain(i => i.Severity == SeverityLevel.High);
        }

        [Fact]
        public async Task GetHighRiskIncidents_ShouldReturnHighRiskIncidents()
        {
            // Arrange - Create high risk incidents
            var highSeverityDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "High severity location",
                Type = IncidentType.Fall,
                Description = "High severity incident",
                Severity = SeverityLevel.High,
                ReportedById = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                EstimatedCost = 1000
            };

            var highCostDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "High cost location",
                Type = IncidentType.Fall,
                Description = "High cost incident",
                Severity = SeverityLevel.Low,
                ReportedById = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                EstimatedCost = 15000
            };

            await _client.PostAsJsonAsync("/api/incident", highSeverityDto);
            await _client.PostAsJsonAsync("/api/incident", highCostDto);

            // Act
            var response = await _client.GetAsync("/api/incident/high-risk");
            var content = await response.Content.ReadAsStringAsync();
            var incidents = JsonSerializer.Deserialize<List<IncidentReadDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            incidents.Should().NotBeNull();
            incidents.Should().HaveCount(2);
            incidents.Should().OnlyContain(i => i.Severity == SeverityLevel.High || i.EstimatedCost > 10000);
        }
    }
} 