# Viana.Api Template

Opinionated `dotnet new` template for ASP.NET Core APIs with Clean Architecture,
JWT auth, Aspire orchestration and a small set of toggles to fit different
shapes of project.

```bash
dotnet new install Viana.Api
dotnet new viana-api -o MyApi --DatabaseProvider sqlserver --EnableJwt true
cd MyApi
dotnet run --project Src/MyApi.AppHost
```

> **Creating from Visual Studio?** Check the **"Place solution and project in the same directory"** option in the New Project wizard. The template is already a multi-project solution with its own `Src/` and `Tests/` layout — if that option is left unchecked, VS wraps everything in an extra `ProjectName/ProjectName/...` folder.

## What you get

- **`Src/MyApi.Api`** — controllers, filters, DI wiring, Swagger, JWT bearer.
- **`Src/MyApi.Application`** — use cases / handlers, interfaces, DTOs,
  validators, entity types.
- **`Src/MyApi.Infrastructure`** — EF Core `DbContext` + factory, password
  hashing, JWT issuance, `ICurrentUser` from HTTP claims.
- **`Src/MyApi.AppHost`** — Aspire host that boots the API plus a containerized
  database for local development.
- **`Tests/MyApi.Tests.Application`** — xUnit + NSubstitute +
  `MockQueryable.NSubstitute` against the `IDbContext` abstraction.
- Optional **`Src/MyApi.Domain`** project when you want entities outside of
  Application.

## Template parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `--DatabaseProvider` | choice | `sqlserver` | `sqlserver` / `postgres` / `mysql`. Wires EF Core provider, Aspire resource and connection string. |
| `--DbContextName` | string | `App` | Prefix for the DbContext type. `App` → `AppDbContext`, `IAppDbContext`, `AppDbContextFactory`. Pass `Foo` to get `FooDbContext` etc. |
| `--UseValidation` | bool | `true` | Adds FluentValidation, the `ValidationActionFilter` and validators for the bundled requests. Auto-registers every `IValidator<T>` via reflection. |
| `--EnableJwt` | bool | `false` | Adds `AuthController` (login / register / refresh), `IJwtService`, password hashing (BCrypt), `[Authorize]` on the demo create/delete, `HttpCurrentUser` and the User/RefreshToken entities. |
| `--EnablePasswordSecurity` | bool | `true` | Only with JWT: turns on the ISO password strength provider used during registration. Strength threshold lives in `appsettings.json` (`PasswordSecurity:MinimumStrength`). |
| `--EnableRateLimiting` | bool | `true` | Adds rate-limiting middleware and the `Login`/`Authenticated` policies referenced by the controllers. |
| `--RunMigrationsOnStartup` | bool | `false` | Calls `DatabaseMigrationRunner` from `Program.cs` so migrations apply at boot. Convenient for dev, avoid in prod. |
| `--ArchitectureStyle` | choice | `cqrs` | `cqrs` → one handler per operation, controllers resolve via `[FromServices]`. `usecase` → controllers ctor-inject each handler. Same files, different wiring. |
| `--UseDomainProject` | bool | `false` | Moves entities, enums and value objects into a separate `MyApi.Domain` project. References are rewritten via `replaces`. |

## Three things that happen "magically"

Read this before changing core wiring.

### 1. Handlers and validators register themselves

`ApplicationServiceCollectionExtensions.AddApplication` scans the Application
assembly:

- Every class implementing `IUseCaseHandler<TRequest, TResponse>` is registered
  as scoped.
- When `UseValidation` is on, every class implementing `IValidator<T>` is
  registered as scoped.

You **do not** add new handlers/validators to a list. Drop the file in the right
folder and it appears in DI.

### 2. `CreatedAt` / `UpdatedAt` / audit user are set by the DbContext

`AppDbContext` overrides `SaveChanges` / `SaveChangesAsync` and walks
`ChangeTracker.Entries<EntityBase>()`:

- `Added` entries get `CreatedAt = DateTimeOffset.UtcNow`.
- `Modified` entries get `UpdatedAt = DateTimeOffset.UtcNow`, and `CreatedAt` is
  marked unmodified (the column cannot be overwritten on update).
- If the entity inherits `AuditableEntity` and the DbContext has a
  `CurrentUserId`, `CreatedByUserId` is set on insert and `UpdatedByUserId` on
  update. `CreatedByUserId` is also marked unmodified on updates.

So handlers should **never** assign `CreatedAt` themselves.

### 3. `IAppDbContextFactory` injects the current user into every context

Handlers depend on `IAppDbContextFactory`, not on `IAppDbContext` directly:

```csharp
public class CreateFooHandler(IAppDbContextFactory dbFactory) : ...
{
    public async Task<...> ExecuteAsync(...)
    {
        await using var db = dbFactory.Create();
        // ... db.CurrentUserId is already populated from the JWT
    }
}
```

`AppDbContextFactory` uses EF Core's `IDbContextFactory<AppDbContext>` to create
fresh, disposable contexts and, when JWT is enabled, calls
`ICurrentUser.UserId` (`HttpCurrentUser` reads `NameIdentifier`/`sub` from the
ambient `HttpContext`) and copies the value onto the new context. That value
flows into the audit logic above.

## Domain conventions

- All entities inherit `EntityBase` → `Guid Id`, `DateTimeOffset CreatedAt`,
  `DateTimeOffset? UpdatedAt`.
- Entities that need to track the acting user inherit `AuditableEntity` →
  adds `Guid CreatedByUserId` (required) and `Guid? UpdatedByUserId` (optional).
  `WeatherForecastEntity` is the bundled example.
- Persistence types end with the `Entity` suffix; `AppDbContext.OnModelCreating`
  strips that suffix from the generated table names
  (`UserEntity` → table `User`).
- `IAppDbContext` exposes only `DbSet<>`s plus `CurrentUserId` and
  `SaveChangesAsync` — no `ChangeTracker` leak.
- Repositories are **not** used. Handlers query the `DbSet<>` directly. One
  `SaveChangesAsync` per use case keeps every write atomic.

## Returning errors

Domain failures use the `Viana.Results` package:

```csharp
return new ProblemResult(409, Messages.Auth.UserWithEmailAlreadyExists);
```

`VianaResultFilter` (registered in `Program.cs`) maps the `Result` envelope to
the right HTTP status. Anything that escapes the MVC pipeline (middleware
exceptions, DB failures, NREs) is caught by `GlobalExceptionHandler`
(`IExceptionHandler`) and rendered as `ProblemDetails`. The client always sees
JSON.

## JWT auth

When `--EnableJwt true`:

- `POST /Auth/register` — creates a user. Optional password-strength gate.
- `POST /Auth/login` — issues access + refresh tokens.
- `POST /Auth/refresh` — exchanges a still-fresh refresh token for a new pair.
  The old refresh token is revoked atomically (rotation).
- JWT signing uses `JsonWebTokenHandler` (Microsoft's current recommendation,
  async validation).
- Access tokens carry `sub`, `email`, `name`, `jti`, `iat`, `exp`.

`appsettings.json` driving it:

```jsonc
"JwtSettings": {
  "Secret": "...",
  "Issuer": "ApiTemplate",
  "Audience": "ApiTemplate",
  "ExpirationMinutes": 15,
  "RefreshTokenExpirationDays": 7
}
```

Override `Secret` via user-secrets / environment for non-local runs.

## EF Core migrations

Generate the first migration:

```bash
dotnet ef migrations add InitialCreate ^
  --project Src/MyApi.Infrastructure/MyApi.Infrastructure.csproj ^
  --startup-project Src/MyApi.Api/MyApi.Api.csproj ^
  --output-dir Data/Migrations
```

Apply:

```bash
dotnet ef database update ^
  --project Src/MyApi.Infrastructure/MyApi.Infrastructure.csproj ^
  --startup-project Src/MyApi.Api/MyApi.Api.csproj
```

Set `--RunMigrationsOnStartup true` (or edit `Program.cs`) if you want the API
to apply pending migrations at boot.

## Validating the template

Maintainers can run all bundled combinations locally:

```pwsh
pwsh scripts/validate-template.ps1
```

The script installs the template from the current checkout, generates a fixed
list of representative combinations (provider × architecture × JWT × Domain
project × DbContext rename × validation/rate-limit toggles), builds each and
runs its test suite. Exit code 1 if anything fails. The same script runs on
every push/PR via `.github/workflows/validate.yml`.

## EF Core versioning

`Directory.Packages.props` exposes `<EFCoreVersion>` and picks per provider:

- SQL Server / PostgreSQL → `10.0.x`
- MySQL → `9.0.x` (Pomelo has no stable EF Core 10 release yet)

Bump in one place.

## License

MIT. See `LICENSE.txt`.
