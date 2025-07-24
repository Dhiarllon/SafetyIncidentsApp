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
                Location = "3rd Floor - North Wing",
                Type = IncidentType.Fall,
                Description = "Height fall",
                Severity = SeverityLevel.Medium,
                ReportedById = Guid.NewGuid(),
                CorrectiveAction = "Implement collective protection",
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
                InvolvedEmployeeId = Guid.NewGuid(),
                EstimatedCost = 5000
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }
    }
} 