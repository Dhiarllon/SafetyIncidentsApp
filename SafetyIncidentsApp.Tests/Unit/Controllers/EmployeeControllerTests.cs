using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using SafetyIncidentsApp.Controllers;
using SafetyIncidentsApp.DTOs;
using SafetyIncidentsApp.Services.Interfaces;
using Xunit;

namespace SafetyIncidentsApp.Tests.Unit.Controllers
{
    public class EmployeeControllerTests
    {
        private readonly Mock<IEmployeeService> _mockEmployeeService;
        private readonly EmployeeController _controller;

        public EmployeeControllerTests()
        {
            _mockEmployeeService = new Mock<IEmployeeService>();
            _controller = new EmployeeController(_mockEmployeeService.Object);
        }

        [Fact]
        public async Task GetEmployees_ShouldReturnOkWithEmployees()
        {
            // Arrange
            var expectedEmployees = new List<EmployeeReadDto>
            {
                new EmployeeReadDto { Id = Guid.NewGuid(), Name = "João Silva" },
                new EmployeeReadDto { Id = Guid.NewGuid(), Name = "Maria Santos" }
            };

            _mockEmployeeService.Setup(s => s.GetAllAsync())
                .ReturnsAsync(expectedEmployees);

            // Act
            var result = await _controller.GetEmployees();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var employees = okResult.Value.Should().BeOfType<List<EmployeeReadDto>>().Subject;
            employees.Should().HaveCount(2);
            employees.Should().BeEquivalentTo(expectedEmployees);
        }

        [Fact]
        public async Task GetEmployee_WithValidId_ShouldReturnOkWithEmployee()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var expectedEmployee = new EmployeeReadDto 
            { 
                Id = employeeId, 
                Name = "João Silva",
                EmployeeCode = "EMP001"
            };

            _mockEmployeeService.Setup(s => s.GetByIdAsync(employeeId))
                .ReturnsAsync(expectedEmployee);

            // Act
            var result = await _controller.GetEmployee(employeeId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var employee = okResult.Value.Should().BeOfType<EmployeeReadDto>().Subject;
            employee.Should().BeEquivalentTo(expectedEmployee);
        }

        [Fact]
        public async Task GetEmployee_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            _mockEmployeeService.Setup(s => s.GetByIdAsync(employeeId))
                .ReturnsAsync((EmployeeReadDto?)null);

            // Act
            var result = await _controller.GetEmployee(employeeId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateEmployee_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var createDto = new EmployeeCreateDto
            {
                Name = "João Silva",
                EmployeeCode = "EMP001",
                Department = "Construção",
                Position = "Pedreiro",
                HireDate = DateTime.Now.AddYears(-2)
            };

            var createdEmployee = new EmployeeReadDto
            {
                Id = Guid.NewGuid(),
                Name = "João Silva",
                EmployeeCode = "EMP001"
            };

            _mockEmployeeService.Setup(s => s.CreateAsync(createDto))
                .ReturnsAsync(createdEmployee);

            // Act
            var result = await _controller.CreateEmployee(createDto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var employee = createdResult.Value.Should().BeOfType<EmployeeReadDto>().Subject;
            employee.Should().BeEquivalentTo(createdEmployee);
            createdResult.ActionName.Should().Be(nameof(EmployeeController.GetEmployee));
            createdResult.RouteValues["id"].Should().Be(createdEmployee.Id);
        }

        [Fact]
        public async Task CreateEmployee_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var createDto = new EmployeeCreateDto();
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = await _controller.CreateEmployee(createDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateEmployee_WithServiceException_ShouldReturnBadRequest()
        {
            // Arrange
            var createDto = new EmployeeCreateDto
            {
                Name = "João Silva",
                EmployeeCode = "EMP001",
                Department = "Construção",
                Position = "Pedreiro",
                HireDate = DateTime.Now.AddYears(-2)
            };

            _mockEmployeeService.Setup(s => s.CreateAsync(createDto))
                .ThrowsAsync(new ArgumentException("Employee code already exists"));

            // Act
            var result = await _controller.CreateEmployee(createDto);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Employee code already exists");
        }
    }
} 