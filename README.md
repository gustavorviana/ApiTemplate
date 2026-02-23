## ApiTemplate

ApiTemplate is a .NET template that creates a clean, opinionated ASP.NET Core solution following a layered architecture:

- **Src/ApiTemplate.Api**: API project (entrypoint, controllers, DI, Swagger/OpenAPI, authentication).
- **Src/ApiTemplate.Application**: Application layer (use cases, interfaces, DTOs, validation).
- **Src/ApiTemplate.Infrastructure**: Infrastructure layer (EF Core, repositories, DbContext, external services).
- **Tests/**: Test projects for application and infrastructure.

The template is parameterized and can be customized when running `dotnet new`:

- **DatabaseProvider**: `sqlserver`, `postgres`, `mysql`, or `none` (no database).
- **UseValidation**: enables FluentValidation integration.
- **EnableJwt**: enables JWT authentication and Auth use cases (only when database is enabled).

When a database provider is selected (anything other than `none`), Entity Framework Core is configured in the Infrastructure project.

### Entity Framework Core migrations

If you selected a database provider when creating the template (`DatabaseProvider` different from `none`), you need to create and apply EF Core migrations before running the API against a real database.

From the solution root, generate the initial migration:

```bash
dotnet ef migrations add InitialCreate ^
  --project Src/ApiTemplate.Infrastructure/ApiTemplate.Infrastructure.csproj ^
  --startup-project Src/ApiTemplate.Api/ApiTemplate.Api.csproj ^
  --output-dir Data/Migrations
```

Then apply the migration to the database:

```bash
dotnet ef database update ^
  --project Src/ApiTemplate.Infrastructure/ApiTemplate.Infrastructure.csproj ^
  --startup-project Src/ApiTemplate.Api/ApiTemplate.Api.csproj
```

Whenever you change the data model (entities/DbContext), create a new migration with a descriptive name, for example:

```bash
dotnet ef migrations add AddNewFeature ^
  --project Src/ApiTemplate.Infrastructure/ApiTemplate.Infrastructure.csproj ^
  --startup-project Src/ApiTemplate.Api/ApiTemplate.Api.csproj ^
  --output-dir Data/Migrations
```

