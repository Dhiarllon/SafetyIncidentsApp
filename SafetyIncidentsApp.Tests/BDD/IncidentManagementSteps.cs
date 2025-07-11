using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using SafetyIncidentsApp.Data;
using SafetyIncidentsApp.DTOs;
using SafetyIncidentsApp.Models;
using System.Net;
using System.Net.Http.Json;
using TechTalk.SpecFlow;

namespace SafetyIncidentsApp.Tests.BDD
{
    [Binding]
    public class IncidentManagementSteps
    {
        private readonly WebApplicationFactory<IApiMarker> _factory;
        private readonly HttpClient _client;
        private IncidentCreateDto _incidentDto;
        private IncidentUpdateDto _updateDto;
        private HttpResponseMessage _response;
        private IncidentReadDto _createdIncident;
        private Guid _incidentId;
        private readonly string _databaseName;

        public IncidentManagementSteps(WebApplicationFactory<IApiMarker> factory)
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

        [Given(@"the safety incidents system is running")]
        public void GivenTheSafetyIncidentsSystemIsRunning()
        {
            _client.Should().NotBeNull();
        }

        [Given(@"there are employees in the system")]
        public void GivenThereAreEmployeesInTheSystem()
        {
            // Employees are seeded in the database
        }

        [Given(@"I want to report a low severity incident")]
        public void GivenIWantToReportALowSeverityIncident()
        {
            _incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                ReportedById = Guid.Parse("11111111-1111-1111-1111-111111111111")
            };
        }

        [Given(@"I want to report a high severity incident")]
        public void GivenIWantToReportAHighSeverityIncident()
        {
            _incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                ReportedById = Guid.Parse("11111111-1111-1111-1111-111111111111")
            };
        }

        [Given(@"I want to report an electric shock incident")]
        public void GivenIWantToReportAnElectricShockIncident()
        {
            _incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Type = IncidentType.ElectricShock,
                ReportedById = Guid.Parse("11111111-1111-1111-1111-111111111111")
            };
        }

        [Given(@"I want to report an incident")]
        public void GivenIWantToReportAnIncident()
        {
            _incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1)
            };
        }

        [Given(@"there is a pending incident requiring approval")]
        public async Task GivenThereIsAPendingIncidentRequiringApproval()
        {
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

            var response = await _client.PostAsJsonAsync("/api/incident", incidentDto);
            _createdIncident = await response.Content.ReadFromJsonAsync<IncidentReadDto>();
            _incidentId = _createdIncident.Id;
        }

        [Given(@"there is an existing incident")]
        public async Task GivenThereIsAnExistingIncident()
        {
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

            var response = await _client.PostAsJsonAsync("/api/incident", incidentDto);
            _createdIncident = await response.Content.ReadFromJsonAsync<IncidentReadDto>();
            _incidentId = _createdIncident.Id;
        }

        [Given(@"there are incidents with different severities")]
        public async Task GivenThereAreIncidentsWithDifferentSeverities()
        {
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
        }

        [Given(@"there are incidents with different risk levels")]
        public async Task GivenThereAreIncidentsWithDifferentRiskLevels()
        {
            var highRiskDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "High risk location",
                Type = IncidentType.Fall,
                Description = "High risk incident",
                Severity = SeverityLevel.High,
                ReportedById = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                EstimatedCost = 5000
            };

            var lowRiskDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Low risk location",
                Type = IncidentType.Other,
                Description = "Low risk incident",
                Severity = SeverityLevel.Low,
                ReportedById = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                EstimatedCost = 500
            };

            await _client.PostAsJsonAsync("/api/incident", highRiskDto);
            await _client.PostAsJsonAsync("/api/incident", lowRiskDto);
        }

        [Given(@"I want to report a PPE violation incident")]
        public void GivenIWantToReportAPPEViolationIncident()
        {
            _incidentDto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Type = IncidentType.ImproperUseOfPPE,
                ReportedById = Guid.Parse("11111111-1111-1111-1111-111111111111")
            };
        }

        [Given(@"the involved employee has not had safety training in the last (.*) months")]
        public void GivenTheInvolvedEmployeeHasNotHadSafetyTrainingInTheLastMonths(int months)
        {
            // This is handled by the test data setup
        }

        [When(@"I create an incident with the following details:")]
        public async Task WhenICreateAnIncidentWithTheFollowingDetails(Table table)
        {
            foreach (var row in table.Rows)
            {
                var field = row["Field"];
                var value = row["Value"];

                switch (field.ToLower())
                {
                    case "location":
                        _incidentDto.Location = value;
                        break;
                    case "type":
                        _incidentDto.Type = Enum.Parse<IncidentType>(value);
                        break;
                    case "description":
                        _incidentDto.Description = value;
                        break;
                    case "severity":
                        _incidentDto.Severity = Enum.Parse<SeverityLevel>(value);
                        break;
                    case "cost":
                        _incidentDto.EstimatedCost = int.Parse(value);
                        break;
                    case "investigationnotes":
                        _incidentDto.InvestigationNotes = value;
                        break;
                }
            }

            _response = await _client.PostAsJsonAsync("/api/incident", _incidentDto);
        }

        [When(@"I create an incident with a non-existent employee")]
        public async Task WhenICreateAnIncidentWithANonExistentEmployee()
        {
            _incidentDto.ReportedById = Guid.NewGuid();
            _response = await _client.PostAsJsonAsync("/api/incident", _incidentDto);
        }

        [When(@"I approve the incident as ""(.*)""")]
        public async Task WhenIApproveTheIncidentAs(string managerName)
        {
            _response = await _client.PostAsync($"/api/incident/{_incidentId}/approve?approvedBy={managerName}", null);
        }

        [When(@"I try to close the incident")]
        public async Task WhenITryToCloseTheIncident()
        {
            _response = await _client.PostAsync($"/api/incident/{_incidentId}/close", null);
        }

        [When(@"I update the incident with new details:")]
        public async Task WhenIUpdateTheIncidentWithNewDetails(Table table)
        {
            _updateDto = new IncidentUpdateDto();

            foreach (var row in table.Rows)
            {
                var field = row["Field"];
                var value = row["Value"];

                switch (field.ToLower())
                {
                    case "location":
                        _updateDto.Location = value;
                        break;
                    case "description":
                        _updateDto.Description = value;
                        break;
                    case "cost":
                        _updateDto.EstimatedCost = int.Parse(value);
                        break;
                }
            }

            _response = await _client.PutAsJsonAsync($"/api/incident/{_incidentId}", _updateDto);
        }

        [When(@"I request incidents with severity ""(.*)""")]
        public async Task WhenIRequestIncidentsWithSeverity(string severity)
        {
            var severityEnum = Enum.Parse<SeverityLevel>(severity);
            _response = await _client.GetAsync($"/api/incident/severity/{severity}");
        }

        [When(@"I request high risk incidents")]
        public async Task WhenIRequestHighRiskIncidents()
        {
            _response = await _client.GetAsync("/api/incident/high-risk");
        }

        [When(@"I create the incident with the untrained employee")]
        public async Task WhenICreateTheIncidentWithTheUntrainedEmployee()
        {
            // Use an employee with old training
            _incidentDto.InvolvedEmployeeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            _response = await _client.PostAsJsonAsync("/api/incident", _incidentDto);
        }

        [Then(@"the incident should be created successfully")]
        public void ThenTheIncidentShouldBeCreatedSuccessfully()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Then(@"the incident status should be ""(.*)""")]
        public async Task ThenTheIncidentStatusShouldBe(string expectedStatus)
        {
            var incident = await _response.Content.ReadFromJsonAsync<IncidentReadDto>();
            incident.Status.ToString().Should().Be(expectedStatus);
        }

        [Then(@"the incident should not require manager approval")]
        public async Task ThenTheIncidentShouldNotRequireManagerApproval()
        {
            var incident = await _response.Content.ReadFromJsonAsync<IncidentReadDto>();
            incident.RequiresManagerApproval.Should().BeFalse();
        }

        [Then(@"the incident should require manager approval")]
        public async Task ThenTheIncidentShouldRequireManagerApproval()
        {
            var incident = await _response.Content.ReadFromJsonAsync<IncidentReadDto>();
            incident.RequiresManagerApproval.Should().BeTrue();
        }

        [Then(@"the incident should not require safety review")]
        public async Task ThenTheIncidentShouldNotRequireSafetyReview()
        {
            var incident = await _response.Content.ReadFromJsonAsync<IncidentReadDto>();
            incident.RequiresSafetyReview.Should().BeFalse();
        }

        [Then(@"the incident should require safety review")]
        public async Task ThenTheIncidentShouldRequireSafetyReview()
        {
            var incident = await _response.Content.ReadFromJsonAsync<IncidentReadDto>();
            incident.RequiresSafetyReview.Should().BeTrue();
        }

        [Then(@"the system should return an error")]
        public void ThenTheSystemShouldReturnAnError()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Then(@"the error message should contain ""(.*)""")]
        public async Task ThenTheErrorMessageShouldContain(string expectedMessage)
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.Should().Contain(expectedMessage);
        }

        [Then(@"the manager approval date should be set")]
        public async Task ThenTheManagerApprovalDateShouldBeSet()
        {
            var incident = await GetIncidentById(_incidentId);
            incident.ManagerApprovalDate.Should().NotBeNull();
        }

        [Then(@"the manager approved by should be ""(.*)""")]
        public async Task ThenTheManagerApprovedByShouldBe(string expectedManager)
        {
            var incident = await GetIncidentById(_incidentId);
            incident.ManagerApprovedBy.Should().Be(expectedManager);
        }

        [Then(@"the incident should be updated successfully")]
        public void ThenTheIncidentShouldBeUpdatedSuccessfully()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Then(@"the incident location should be ""(.*)""")]
        public async Task ThenTheIncidentLocationShouldBe(string expectedLocation)
        {
            var incident = await _response.Content.ReadFromJsonAsync<IncidentReadDto>();
            incident.Location.Should().Be(expectedLocation);
        }

        [Then(@"the incident description should be ""(.*)""")]
        public async Task ThenTheIncidentDescriptionShouldBe(string expectedDescription)
        {
            var incident = await _response.Content.ReadFromJsonAsync<IncidentReadDto>();
            incident.Description.Should().Be(expectedDescription);
        }

        [Then(@"the incident cost should be (.*)")]
        public async Task ThenTheIncidentCostShouldBe(int expectedCost)
        {
            var incident = await _response.Content.ReadFromJsonAsync<IncidentReadDto>();
            incident.EstimatedCost.Should().Be(expectedCost);
        }

        [Then(@"I should receive only high severity incidents")]
        public async Task ThenIShouldReceiveOnlyHighSeverityIncidents()
        {
            var incidents = await _response.Content.ReadFromJsonAsync<List<IncidentReadDto>>();
            incidents.Should().OnlyContain(i => i.Severity == SeverityLevel.High);
        }

        [Then(@"all returned incidents should have severity ""(.*)""")]
        public async Task ThenAllReturnedIncidentsShouldHaveSeverity(string expectedSeverity)
        {
            var incidents = await _response.Content.ReadFromJsonAsync<List<IncidentReadDto>>();
            var severityEnum = Enum.Parse<SeverityLevel>(expectedSeverity);
            incidents.Should().OnlyContain(i => i.Severity == severityEnum);
        }

        [Then(@"I should receive incidents with high severity or cost over 10000")]
        public async Task ThenIShouldReceiveIncidentsWithHighSeverityOrCostOver10000()
        {
            var incidents = await _response.Content.ReadFromJsonAsync<List<IncidentReadDto>>();
            incidents.Should().OnlyContain(i => i.Severity == SeverityLevel.High || i.EstimatedCost > 10000);
        }

        [Then(@"all returned incidents should be high risk")]
        public async Task ThenAllReturnedIncidentsShouldBeHighRisk()
        {
            var incidents = await _response.Content.ReadFromJsonAsync<List<IncidentReadDto>>();
            incidents.Should().OnlyContain(i => i.Severity == SeverityLevel.High || i.EstimatedCost > 10000);
        }

        private async Task<IncidentReadDto> GetIncidentById(Guid id)
        {
            var response = await _client.GetAsync($"/api/incident/{id}");
            return await response.Content.ReadFromJsonAsync<IncidentReadDto>();
        }
    }
} 