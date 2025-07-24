# Safety Incidents API

Uma API REST para gerenciamento de incidentes de segurança no trabalho, demonstrando boas práticas de desenvolvimento.

## 🎯 Objetivo

Demonstrar conceitos de:
- **Clean Architecture** com separação de responsabilidades
- **TDD (Test-Driven Development)** com testes unitários e BDD
- **Regras de Negócio** aplicadas no domínio
- **Validações** e tratamento de erros
- **Entity Framework Core** com relacionamentos

## 🏗️ Arquitetura

```
Controllers/     # API Endpoints
Services/        # Lógica de negócio
Repositories/    # Acesso a dados
Models/          # Entidades do domínio
DTOs/           # Objetos de transferência
Tests/          # Testes unitários e BDD
```

## 🚀 Funcionalidades Principais

### Incidentes
- ✅ Criar incidente com regras de negócio
- ✅ Aprovar incidentes de alta severidade
- ✅ Fechar incidentes
- ✅ Buscar por severidade
- ✅ Listar pendentes de aprovação

### Regras de Negócio
- **Alta Severidade**: Requer aprovação do gerente
- **Média Severidade + Custo > 5000**: Requer aprovação
- **Baixa Severidade**: Aprovação automática

## 🧪 Testes

### Testes Unitários
```bash
dotnet test SafetyIncidentsApp.Tests/Unit/
```

### Testes BDD (SpecFlow)
```bash
dotnet test SafetyIncidentsApp.Tests/BDD/
```

## 📊 Exemplos de Uso

### Criar Incidente de Baixa Severidade
```json
POST /api/incident
{
  "date": "2024-01-15T10:00:00Z",
  "location": "Andar 2 - Ala Norte",
  "description": "Escorregou no piso molhado",
  "type": "Fall",
  "severity": "Low",
  "reportedById": "employee-guid",
  "estimatedCost": 500
}
```

### Criar Incidente de Alta Severidade
```json
POST /api/incident
{
  "date": "2024-01-15T10:00:00Z",
  "location": "Andar 10 - Ala Sul",
  "description": "Queda de altura crítica",
  "type": "Fall",
  "severity": "High",
  "reportedById": "employee-guid",
  "estimatedCost": 15000
}
```

## 🛠️ Tecnologias

- **.NET 8** - Framework principal
- **Entity Framework Core** - ORM
- **AutoMapper** - Mapeamento de objetos
- **FluentAssertions** - Assertions expressivas
- **Moq** - Mocking para testes
- **SpecFlow** - Testes BDD
- **SQLite** - Banco de dados (desenvolvimento)

## 🚀 Executar

```bash
# Restaurar dependências
dotnet restore

# Executar migrações
dotnet ef database update

# Executar aplicação
dotnet run --project SafetyIncidentsApp

# Executar testes
dotnet test
```

## 📝 Swagger

Acesse a documentação da API em: `https://localhost:7001/swagger` 