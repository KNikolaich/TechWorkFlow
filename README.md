# TechWorkFlow

TechWorkFlow - система планирования, распределения и контроля заявок на обслуживание оборудования.

## Stack

- Backend: ASP.NET Core Web API (.NET 8)
- Frontend: Blazor WebAssembly
- Database: PostgreSQL (primary), SQL Server / SQLite (switch by config)
- ORM: EF Core 8 (Code-First)
- Auth: ASP.NET Core Identity + JWT
- Tests: xUnit + FluentAssertions + Moq + Testcontainers

## Solution Structure

- `Backend` - Web API
- `Frontend` - Blazor WASM client
- `Shared` - DTO/contracts/domain models
- `Infrastructure` - EF Core/persistence/services
- `Common` - common constants/settings
- `Backend.Tests` - unit and integration tests

## Local Run (without Docker)

1. Configure secrets for Backend:
   - `ConnectionStrings:DefaultConnection`
   - `Jwt:SigningKey`
2. Build:
   - `dotnet build TechWorkFlow.sln`
3. Run API:
   - `dotnet run --project Backend/Backend.csproj`
4. Run client:
   - `dotnet run --project Frontend/Frontend.csproj`

On API startup, migrations are applied automatically and bootstrap roles/users are created:
- Roles: `Admin`, `Supervisor`, `Worker`
- Admin account comes from `AdminBootstrap` section in `appsettings`/secrets.

## Docker Run

1. Create env file:
   - copy `.env.example` to `.env`
2. Start containers:
   - `docker compose up --build`
3. Open:
   - Backend: `http://localhost:8080`
   - Frontend: `http://localhost:8081`

## Tests

- All tests:
  - `dotnet test Backend.Tests/Backend.Tests.csproj`
- Integration tests use PostgreSQL Testcontainers.
  - If Docker is unavailable, container-based tests are skipped.

## EF Core Migrations

- Initial migration created in `Infrastructure/Persistence/Migrations`.
- Create next migration:
  - `D:\Docs\!Projects\.tools\dotnet-ef.exe migrations add <Name> --project Infrastructure/Infrastructure.csproj --startup-project Backend/Backend.csproj --context Infrastructure.Persistence.AppDbContext --output-dir Persistence/Migrations`
