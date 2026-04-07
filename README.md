# LightningArc.Utils

**LightningArc** is a comprehensive set of C# utilities and patterns designed to promote clean, expressive, and robust code in .NET applications. It focuses on Functional Error Handling, Domain-Driven Design primitives, seamless ASP.NET Core integration, and compile-time static analysis.

## Documentation

Detailed documentation is available in the **[Docs](docs/README.md)** directory:

- **[Getting Started](docs/getting-started/introduction.md)**: Project overview and philosophy.
- **[Core Features](docs/core-features/result-pattern.md)**: Result pattern, Value Objects, and more.
- **[Data Access](docs/data-access/abstractions.md)**: Repository and Unit of Work abstractions.
- **[Web Integration](docs/web-integration/results-mapping.md)**: HTTP mapping, CORS, OpenAPI.
- **[Analyzers](docs/analyzers/README.md)**: 9 compile-time rules and 2 code fixes.
- **[AI Skill](docs/skill/SKILL.md)**: Context for AI assistants (Claude Code, Gemini, etc.).

## Modules

The solution is organized into four layers:

### Core Layer
Foundation types and patterns, framework-agnostic.

| Project | Description |
|---|---|
| **`LightningArc.Results`** | Complete Result pattern (Success/Failure) with chaining operators. |
| **`LightningArc.Primitives`** | Value Objects: `Email`, `Cpf`, `Cnpj`, `PhoneNumber`, `Url`. |
| **`LightningArc.Primitives.Results`** | Result-based extensions for ValueObjects. |
| **`LightningArc.Json`** | `System.Text.Json` converters and helpers. |
| **`LightningArc.Core`** | General utilities and helpers. |

### Data Layer
Repository patterns and ADO.NET helpers.

| Project | Description |
|---|---|
| **`LightningArc.Data.Abstractions`** | `IRepository`, `IUnitOfWork`, `IConnectionFactory` interfaces. |
| **`LightningArc.Data.ADO`** | Base `RepositoryBase` with `GetConnectionAsync` support. |
| **`LightningArc.Data.ADO.SqlServer`** | `SqlConnectionFactory` for SQL Server. |
| **`LightningArc.Data.ADO.Oracle`** | `OracleConnectionFactory` for Oracle. |
| **`LightningArc.Data.ADO.SqlBuilder`** | Metadata-driven SQL statement builder. |
| **`LightningArc.Data.EntityFramework`** | EF Core repository base. |
| **`LightningArc.Mappers.AutoMapper`** | AutoMapper adapter for data layer. |
| **`LightningArc.Mappers.Mapster`** | Mapster adapter for data layer. |

### Web Layer
ASP.NET Core integrations.

| Project | Description |
|---|---|
| **`LightningArc.AspNetCore`** | Core ASP.NET Core utilities. |
| **`LightningArc.Results.AspNetCore`** | Map `Result<T>` to HTTP responses (RFC 7807 Problem Details). |
| **`LightningArc.CORS.AspNetCore`** | Pre-configured CORS policies. |
| **`LightningArc.OpenAPI.AspNetCore`** | OpenAPI/Swagger transformers for ValueObjects. |

### Meta & Analyzers
Compile-time tools.

| Project | Description |
|---|---|
| **`LightningArc.Analyzers`** | 9 Roslyn analyzer rules + 2 automatic code fixes. |
| **`LightningArc.Metalama`** | Metalama-powered AOP factories. |
| **`LightningArc.Metalama.Results`** | Result-related Metalama utilities. |

## Quick Start

### Error Handling with Results

```csharp
using LightningArc.Results;

public Result<User> CreateUser(string email, string password)
{
    if (string.IsNullOrEmpty(email))
        return Error.Validation.MissingField("Email is required");

    if (password.Length < 8)
        return Error.Validation.ValueOutOfRange("Password must be at least 8 characters");

    return Result.Success(new User(email, password));
}
```

### Safe Value Object Creation

```csharp
using LightningArc.Primitives;
using LightningArc.Primitives.Results;

// Recommended: TryCreate won't throw
if (Email.TryCreate(input, out var email)) { /* use it */ }

// Result-based: returns Failure on invalid input
Result<Email> result = input.CreateEmailResult();
if (result.IsSuccess) { /* use result.Value */ }

// Direct: throws ArgumentException on invalid input
var email = Email.Create(input);
```

### ASP.NET Core Integration

```csharp
// Program.cs
builder.Services.AddEndpointResults();

// Endpoint — Result<T> maps to 200 OK or 4xx automatically
app.MapPost("/users", (UserRequest req) => userService.Create(req));
```

### Using the Analyzers

All projects in the solution automatically reference `LightningArc.Analyzers` via `Directory.Build.props` — no setup needed. For external projects consuming the library via NuGet, reference the analyzer package once published.

## Building

The project uses centralized package management and MSBuild props. Build with:

```bash
dotnet build
```

## License

This project is proprietary/private.
