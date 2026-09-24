# Employee Admin Portal API

![CI](https://github.com/tnikhil-24/EmployeeAdminPortal/actions/workflows/ci.yml/badge.svg)

A RESTful ASP.NET Core Web API for managing employees, built with .NET 10, Entity Framework Core and SQL Server, using a layered architecture with dependency injection, validation, centralized error handling and unit tests.

## Tech Stack

- **.NET 10** / **C# 14** / **ASP.NET Core Web API**
- **Entity Framework Core** with **SQL Server** (LocalDB for development), code-first migrations
- **OpenAPI** (built-in) with **Swagger UI**
- **xUnit** + **Moq** for unit testing
- **GitHub Actions** for continuous integration

## Architecture

```
Controller  →  Service  →  Repository  →  DbContext  →  SQL Server
  (HTTP)      (business     (data         (EF Core)
               rules)        access)
```

- **Controllers** handle HTTP only: routing, status codes, request/response DTOs.
- **Services** contain business rules (e.g. email must be unique) and map entities to DTOs.
- **Repositories** encapsulate all EF Core queries.
- Each layer depends on an **interface** (`IEmployeeService`, `IEmployeeRepository`), registered with **Scoped** lifetime in the built-in DI container.

## Features

- Full CRUD for employees with async EF Core queries (`AsNoTracking` for reads)
- **DTOs** for input and output, so the API contract is decoupled from the database schema
- **Validation** with DataAnnotations; invalid requests return `400` automatically
- **Correct HTTP semantics**: `201 Created` with a `Location` header, `204 No Content`, `404`, `409 Conflict`
- **Unique email** enforced in the service layer and by a unique database index
- **Global exception handling** via `IExceptionHandler`: every error returns RFC 7807 **ProblemDetails**; `500` responses hide internal details while the full exception is logged

## API Endpoints

| Method | Endpoint | Description | Responses |
|---|---|---|---|
| GET | `/api/Employees` | List all employees | 200 |
| GET | `/api/Employees/{id}` | Get one employee | 200, 404 |
| POST | `/api/Employees` | Create an employee | 201, 400, 409 |
| PUT | `/api/Employees/{id}` | Update an employee | 200, 400, 404, 409 |
| DELETE | `/api/Employees/{id}` | Delete an employee | 204, 404 |

**Example request (POST):**
```json
{
  "name": "Jane Doe",
  "email": "jane.doe@example.com",
  "phone": "555-0100",
  "salary": 85000
}
```

**Example error (409):**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Email already in use",
  "status": 409,
  "detail": "An employee with the email 'jane.doe@example.com' already exists.",
  "traceId": "00-..."
}
```

## Getting Started

**Prerequisites:** .NET 10 SDK, SQL Server LocalDB (installed with Visual Studio)

```bash
git clone https://github.com/tnikhil-24/EmployeeAdminPortal.git
cd EmployeeAdminPortal

# Create the database from migrations
dotnet tool install --global dotnet-ef
dotnet ef database update --project EmployeeAdminPortal

# Run the API
dotnet run --project EmployeeAdminPortal --launch-profile https
```

Open **https://localhost:7291/swagger** to explore the API.

The connection string is in `EmployeeAdminPortal/appsettings.json`.

## Running Tests

```bash
dotnet test
```

The service layer is unit tested with the repository mocked (10 tests covering success, not-found and duplicate-email paths), so tests run without a database. The same tests run on every push via GitHub Actions.

## Project Structure

```
EmployeeAdminPortal/
├── Controllers/        # HTTP endpoints
├── Services/           # Business logic (IEmployeeService, EmployeeService)
├── Repositories/       # Data access (IEmployeeRepository, EmployeeRepository)
├── Data/               # ApplicationDbContext
├── Models/             # Entities and DTOs
├── Exceptions/         # Domain exceptions
├── Middleware/         # GlobalExceptionHandler
└── Migrations/         # EF Core migrations
EmployeeAdminPortal.Tests/   # xUnit + Moq unit tests
```

## Possible Next Steps

- Integration tests with `WebApplicationFactory` and SQL Server in Testcontainers
- Pagination and filtering on `GET /api/Employees`
- JWT authentication and role-based authorization
- React frontend consuming the API
