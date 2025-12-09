# Technical Test - Norlys Energy Trading

A RESTful API built with ASP.NET Core for managing person records and their office assignments. This project was created as part of a technical assessment for Norlys Energy Trading.
(README is AI generated)

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Architecture & Workflow](#architecture--workflow)
- [API Endpoints](#api-endpoints)
- [Database Schema](#database-schema)
- [Validation Rules](#validation-rules)
- [Configuration](#configuration)

---

## Overview

This application provides a simple Person Management System that allows users to:

- **Create** new person records with office assignments
- **Read** all persons or individual person details
- **Update** existing person records (partial updates supported)
- **Delete** person records

Each person is associated with an office location, enforcing referential integrity through foreign key relationships.

---

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server (local instance on port 1433)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/kevinpedersen/Technical-Test-Norlys.git
   cd Technical-Test-Norlys
   ```

2. **Configure database connection**
   Update `TechnicalTestNorlys/appsettings.json` with your SQL Server credentials:
   ```json
   {
       "UserId": "sa",
       "Password": "YourPasswordHere"
   }
   ```

3. **Run the application**
   ```bash
   cd TechnicalTestNorlys
   dotnet run
   ```

4. **Access Swagger UI**
   Navigate to `http://localhost:5000/swagger` to explore and test the API.

---

## Technology Stack

| Component | Technology |
|-----------|------------|
| **Framework** | ASP.NET Core (.NET 9.0) |
| **Database** | SQL Server |
| **ORM** | Entity Framework Core 9.0 |
| **Validation** | FluentValidation 12.1 |
| **API Documentation** | Swagger/OpenAPI (Swashbuckle) |

---

## Project Structure

```
TechnicalTestNorlys/
├── API/                    # REST API Controllers (Endpoints)
│   ├── AddPersonEndpoint.cs
│   ├── DeletePersonEndpoint.cs
│   ├── GetPersonsEndpoint.cs
│   └── UpdatePersonEndpoint.cs
├── Entities/               # Request DTOs
│   ├── AddPersonRequest.cs
│   └── UpdatePersonRequest.cs
├── Handlers/               # Business Logic Layer
│   └── PersonHandler.cs
├── Models/                 # Domain Entities
│   ├── Office.cs
│   └── Person.cs
├── Repositories/           # Data Access Layer
│   ├── IPersonRepository.cs
│   └── PersonRepository.cs
├── Validators/             # Input Validation
│   ├── AddPersonValidator.cs
│   └── UpdatePersonValidator.cs
├── Migrations/             # EF Core Migrations
├── AppDbContext.cs         # Database Context
├── Program.cs              # Application Entry Point
└── appsettings.json        # Configuration
```

---

## Architecture & Workflow

The application follows a **layered architecture** pattern with clear separation of concerns:

### Request Flow Pipeline

```
HTTP Request → Controller → Validator → Handler → Repository → Database
                    ↓
              HTTP Response ← Controller ← Handler ← Repository ←
```

### Layer Responsibilities

#### 1. **API Layer** (`/API`)
Controllers receive HTTP requests and return responses. Each endpoint:
- Handles a specific CRUD operation
- Validates incoming requests using FluentValidation
- Delegates business logic to handlers
- Manages error handling and logging
- Returns appropriate HTTP status codes

#### 2. **Entities Layer** (`/Entities`)
Contains Data Transfer Objects (DTOs) for API requests:
- `AddPersonRequest`: Required fields for creating a person
- `UpdatePersonRequest`: Optional fields for partial updates (PATCH behavior)

#### 3. **Handlers Layer** (`/Handlers`)
Business logic orchestration:
- Acts as an intermediary between controllers and repositories
- Provides logging at the business logic level
- Can be extended for additional business rules

#### 4. **Repositories Layer** (`/Repositories`)
Data access abstraction:
- Implements the Repository pattern with interface `IPersonRepository`
- Handles all Entity Framework Core operations
- Manages entity relationships and navigation properties
- Validates office existence before person creation/update

#### 5. **Models Layer** (`/Models`)
Domain entities representing database tables:
- `Person`: Employee record with office foreign key
- `Office`: Office location with capacity

#### 6. **Validators Layer** (`/Validators`)
FluentValidation rules for input validation:
- Enforces data integrity at the API boundary
- Provides descriptive error messages
- Validates age requirements (13-120 years)
- Prevents whitespace in last names

---

## API Endpoints

### Person Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/add-person` | Create a new person |
| `GET` | `/get-persons` | Get all persons |
| `GET` | `/get-persons/{id}` | Get person by ID |
| `PATCH` | `/update-persons/{id}` | Update person (partial) |
| `DELETE` | `/delete-person/{id}` | Delete person |

### Request/Response Examples

#### Create Person
```json
POST /add-person
{
    "firstName": "John",
    "lastName": "Doe",
    "dateOfBirth": "1990-05-15",
    "office": "Silkeborg"
}
```

#### Update Person (Partial)
```json
PATCH /update-persons/1
{
    "firstName": "Jane",
    "office": "Aalborg"
}
```

#### Response Format
```json
{
    "id": 1,
    "firstName": "Jane",
    "lastName": "Doe",
    "dateOfBirth": "1990-05-15",
    "office": "Aalborg"
}
```

---

## Database Schema

### Office Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | Primary Key |
| Location | string(100) | Required, Unique |
| Capacity | int | - |

**Seed Data:**
- Silkeborg (Capacity: 5)
- Esbjerg (Capacity: 10)
- Aalborg (Capacity: 50)

### Person Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | Primary Key |
| FirstName | string | Required |
| LastName | string | Required |
| DateOfBirth | DateOnly | Required |
| OfficeId | int | Foreign Key → Office.Id |

---

## Validation Rules

### Add Person
| Field | Rules |
|-------|-------|
| FirstName | Required, Max 50 characters |
| LastName | Required, Max 50 characters, No whitespace |
| DateOfBirth | Must be 13-120 years old |
| Office | Required, Max 100 characters |

### Update Person
All fields are optional (PATCH behavior), but when provided:

| Field | Rules |
|-------|-------|
| FirstName | Max 50 characters |
| LastName | Max 50 characters, No whitespace |
| DateOfBirth | Must be 13-120 years old |
| Office | Max 100 characters |

---

## Configuration

### Database Connection
The application connects to SQL Server on `localhost:1433` with database name `TestDatabase`. The connection string is built dynamically from `appsettings.json`:

```
Server=localhost,1433;Database=TestDatabase;User Id={UserId};Password={Password};TrustServerCertificate=True
```

### Logging
Configured logging levels in `appsettings.json`:
- Default: Debug
- System: Information
- Microsoft: Information

Each layer logs with prefixes (`[Controller]`, `[Handler]`, `[Repository]`, `[Validator]`) for easy debugging.

---

## Key Design Decisions

1. **Repository Pattern**: Abstracts data access, enabling easier testing and potential database swapping.

2. **FluentValidation**: Provides declarative, readable validation rules separate from controllers.

3. **PATCH Support**: Update endpoint accepts partial updates, only modifying provided fields.

4. **Office Validation**: Ensures referential integrity by validating office existence before person creation/update.

5. **Structured Logging**: Each layer logs with consistent prefixes for easy debugging and tracing.

---

## Future Improvements

- Add unit and integration tests
- Implement office capacity validation
- Add pagination for GetAllPersons endpoint
- Implement authentication/authorization
- Add response DTOs for consistent API contracts
- Implement global exception handling middleware
- Add Docker support for containerized deployment

---

## License

This project was created for a technical assessment and is not licensed for production use.

