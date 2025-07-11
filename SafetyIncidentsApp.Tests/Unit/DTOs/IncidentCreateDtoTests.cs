using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using SafetyIncidentsApp.DTOs;
using SafetyIncidentsApp.Models;
using Xunit;

namespace SafetyIncidentsApp.Tests.Unit.DTOs
{
    public class IncidentCreateDtoTests
    {
        [Fact]
        public void IncidentCreateDto_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var dto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Andar 3 - Ala Norte",
                Type = IncidentType.Fall,
                Description = "Queda de altura",
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.NewGuid(),
                CorrectiveAction = "Implementar proteção coletiva",
                EstimatedCost = 5000
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void IncidentCreateDto_WithMissingRequiredFields_ShouldFailValidation()
        {
            // Arrange
            var dto = new IncidentCreateDto
            {
                // Missing required fields
                Description = "Test description"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(v => v.MemberNames.Contains("Date"));
            validationResults.Should().Contain(v => v.MemberNames.Contains("Location"));
            validationResults.Should().Contain(v => v.MemberNames.Contains("Type"));
            validationResults.Should().Contain(v => v.MemberNames.Contains("Severity"));
            validationResults.Should().Contain(v => v.MemberNames.Contains("ReportedById"));
        }

        [Fact]
        public void IncidentCreateDto_WithNullLocation_ShouldFailValidation()
        {
            // Arrange
            var dto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = null!,
                Type = IncidentType.Fall,
                Description = "Test description",
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.NewGuid()
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(v => v.MemberNames.Contains("Location"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void IncidentCreateDto_WithInvalidLocation_ShouldFailValidation(string location)
        {
            // Arrange
            var dto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = location,
                Type = IncidentType.Fall,
                Description = "Test description",
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.NewGuid()
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(v => v.MemberNames.Contains("Location"));
        }

        [Fact]
        public void IncidentCreateDto_WithLocationTooLong_ShouldFailValidation()
        {
            // Arrange
            var dto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = new string('A', 201), // Exceeds 200 characters
                Type = IncidentType.Fall,
                Description = "Test description",
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.NewGuid()
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(v => v.MemberNames.Contains("Location"));
        }

        [Fact]
        public void IncidentCreateDto_WithDescriptionTooLong_ShouldFailValidation()
        {
            // Arrange
            var dto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test location",
                Type = IncidentType.Fall,
                Description = new string('A', 1001), // Exceeds 1000 characters
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.NewGuid()
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(v => v.MemberNames.Contains("Description"));
        }

        [Fact]
        public void IncidentCreateDto_WithNegativeEstimatedCost_ShouldFailValidation()
        {
            // Arrange
            var dto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test location",
                Type = IncidentType.Fall,
                Description = "Test description",
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.NewGuid(),
                EstimatedCost = -1000
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(v => v.MemberNames.Contains("EstimatedCost"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1000)]
        [InlineData(50000)]
        public void IncidentCreateDto_WithValidEstimatedCost_ShouldPassValidation(int cost)
        {
            // Arrange
            var dto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test location",
                Type = IncidentType.Fall,
                Description = "Test description",
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.NewGuid(),
                EstimatedCost = cost
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Theory]
        [InlineData(IncidentType.Fall)]
        [InlineData(IncidentType.ElectricShock)]
        [InlineData(IncidentType.ImproperUseOfPPE)]
        [InlineData(IncidentType.Collision)]
        [InlineData(IncidentType.Other)]
        public void IncidentCreateDto_WithValidIncidentTypes_ShouldPassValidation(IncidentType type)
        {
            // Arrange
            var dto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test location",
                Type = type,
                Description = "Test description",
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.NewGuid()
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Theory]
        [InlineData(SeverityLevel.Low)]
        [InlineData(SeverityLevel.Medium)]
        [InlineData(SeverityLevel.High)]
        public void IncidentCreateDto_WithValidSeverityLevels_ShouldPassValidation(SeverityLevel severity)
        {
            // Arrange
            var dto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test location",
                Type = IncidentType.Fall,
                Description = "Test description",
                Severity = severity,
                ReportedById = Guid.NewGuid()
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void IncidentCreateDto_WithValidOptionalFields_ShouldPassValidation()
        {
            // Arrange
            var dto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test location",
                Type = IncidentType.Fall,
                Description = "Test description",
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.NewGuid(),
                CorrectiveAction = "Test corrective action",
                InvestigationNotes = "Test investigation notes",
                Witnesses = "Test witnesses",
                InvolvedEmployeeId = Guid.NewGuid(),
                SafetyInspectionId = Guid.NewGuid(),
                EstimatedCost = 1000,
                IsNearMiss = true
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void IncidentCreateDto_WithCorrectiveActionTooLong_ShouldFailValidation()
        {
            // Arrange
            var dto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test location",
                Type = IncidentType.Fall,
                Description = "Test description",
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.NewGuid(),
                CorrectiveAction = new string('A', 501) // Exceeds 500 characters
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(v => v.MemberNames.Contains("CorrectiveAction"));
        }

        [Fact]
        public void IncidentCreateDto_WithInvestigationNotesTooLong_ShouldFailValidation()
        {
            // Arrange
            var dto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test location",
                Type = IncidentType.Fall,
                Description = "Test description",
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.NewGuid(),
                InvestigationNotes = new string('A', 1001) // Exceeds 1000 characters
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(v => v.MemberNames.Contains("InvestigationNotes"));
        }

        [Fact]
        public void IncidentCreateDto_WithWitnessesTooLong_ShouldFailValidation()
        {
            // Arrange
            var dto = new IncidentCreateDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                Location = "Test location",
                Type = IncidentType.Fall,
                Description = "Test description",
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.NewGuid(),
                Witnesses = new string('A', 501) // Exceeds 500 characters
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(v => v.MemberNames.Contains("Witnesses"));
        }
    }
} 