# Safety Incidents API

A REST API for managing workplace safety incidents, demonstrating best development practices.

## 🎯 Purpose

Demonstrate concepts of:
- **Clean Architecture** with separation of concerns
- **TDD (Test-Driven Development)** with unit and BDD tests
- **Business Rules** applied in the domain
- **Validations** and error handling
- **Entity Framework Core** with relationships

## 🏗️ Architecture

```
Controllers/     # API Endpoints
Services/        # Business logic
Repositories/    # Data access
Models/          # Domain entities
DTOs/            # Data transfer objects
Tests/           # Unit and BDD tests
```

## 🚀 Main Features

### Incidents
- ✅ Create incident with business rules
- ✅ Approve high severity incidents
- ✅ Close incidents
- ✅ Search by severity
- ✅ List pending approval

### Business Rules
- **High Severity**: Requires manager approval
- **Medium Severity + Cost > 5000**: Requires approval
- **Low Severity**: Automatic approval

## 🧪 Tests

### Unit Tests
```bash
dotnet test SafetyIncidentsApp.Tests/Unit/
```

### BDD Tests (SpecFlow)
```bash
dotnet test SafetyIncidentsApp.Tests/BDD/
```

## 📊 Usage Examples

### Create Low Severity Incident
```json
POST /api/incident
{
  "date": "2024-01-15T10:00:00Z",
  "location": "2nd Floor - North Wing",
  "description": "Slipped on wet floor",
  "type": "Fall",
  "severity": "Low",
  "reportedById": "employee-guid",
  "estimatedCost": 500
}
```

### Create High Severity Incident
```json
POST /api/incident
{
  "date": "2024-01-15T10:00:00Z",
  "location": "10th Floor - South Wing",
  "description": "Critical height fall",
  "type": "Fall",
  "severity": "High",
  "reportedById": "employee-guid",
  "estimatedCost": 15000
}
```

## 🛠️ Technologies

- **.NET 8** - Main framework
- **Entity Framework Core** - ORM
- **AutoMapper** - Object mapping
- **FluentAssertions** - Expressive assertions
- **Moq** - Mocking for tests
- **SpecFlow** - BDD tests
- **SQLite** - Database (development)

## 🚀 Run

```bash
# Restore dependencies
dotnet restore

# Run migrations
dotnet ef database update

# Run application
dotnet run --project SafetyIncidentsApp

# Run tests
dotnet test
```

## 📝 Swagger

Access the API documentation at: `https://localhost:7001/swagger` 