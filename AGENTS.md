# LightningArc.Framework — Agent Instructions

Working conventions for AI assistants operating in this repository.

---

## Project Overview

**LightningArc** is a modular .NET framework ecosystem focused on:
- Functional error handling (`Result<T>` pattern)
- Domain-Driven Design Value Objects (`Email`, `Cpf`, `Cnpj`, `PhoneNumber`, `Url`)
- Repository pattern with ADO.NET/EF Core support
- ASP.NET Core integration (HTTP mapping, OpenAPI)
- Roslyn static analyzers
- Licensed under Apache License 2.0

---

## Architecture Layers

### Core (`src/Core/`)
- **Primitives** — Value objects, `IValueObject<T>`, validation
- **Results** — `Result<T>`, `Result` (non-generic), `Error` hierarchy, `AggregateError`, LINQ-style chaining (`Bind`, `Map`, `Ensure`, `Tap`, `OnFailure`, `Match`), `TryGetValue`, `TryGetError`
- **Primitives.Results** — Extensions for ValueObjects that return `Result<T>` instead of throwing
- **Json** — `System.Text.Json` converters and helpers
- **Core** — General utilities

### Data (`src/Data/`)
- **Data.Abstractions** — `IRepository`, `IUnitOfWork`, `IConnectionFactory` interfaces
- **Data.ADO** — `RepositoryBase`, `GetConnection()` + `GetConnectionAsync()`, `ReleaseConnection()`
- **Data.ADO.SqlServer**, **Data.ADO.Oracle**, **Data.ADO.SqlBuilder** — Providers
- **Data.EntityFramework** — EF Core repository base
- **Mappers.AutoMapper/Mappers.Mapster** — Mapper adapters

### Web (`src/Web/`)
- **AspNetCore** — Core ASP.NET Core utilities
- **Results.AspNetCore** — Maps `Result<T>` to HTTP responses (RFC 7807)
- **OpenAPI.AspNetCore** — OpenAPI/Swagger transformers for ValueObjects
- **CORS.AspNetCore** — Pre-configured CORS policies

### Meta (`src/Meta/`)
- **Metalama** — AOP factories
- **Metalama.Results** — Result-related Metalama utilities

### Analyzers (`src/Analyzers/`)
- **9 rules** (LARC001–030) + 2 code fix providers
- Auto-referenced by all library projects via `src/Directory.Build.props`
- See `docs/analyzers/README.md` for rule details

### Tests (`tests/`)
- TUnit framework (NOT xUnit — xUnit was fully removed)
- Moq for mocking

---

## Key Conventions

### Build & Config
- **Central Package Management** — `Directory.Packages.props` for all versions, see the file for current versions.
- **Centralized build props** — `Directory.Build.props` (root) and `src/Directory.Build.props`
- **Multi-targeting** — `netstandard2.0`, `net9.0`, `net10.0` (varies by project)
- **Artifacts** — Output to `.artifacts/` folder
- **Pack on build** — `GeneratePackageOnBuild=true` for all projects

## Test Running Rules
- Use `dotnet test` for all tests.
- Use `dotnet test --project "tests/<Path>/<Project>.csproj"` for a specific project.
- Never use VSTest `--filter` in this repo.
- Use TUnit `--treenode-filter` syntax when filtering is needed. Examples:
    All tests in a class: `dotnet test -- --treenode-filter "/*/*/MyTestClass/*"`
    A specific test method: `dotnet test -- --treenode-filter "/*/*/MyTestClass/MyTestMethod"`
    By category: `dotnet test -- --treenode-filter "/*/*/*/*[Category=Integration]"`
    Exclude a category: `dotnet test -- --treenode-filter "/*/*/*/*[Category!=Performance]"`
    Multiple filters (OR): `dotnet test -- --treenode-filter "/*/*/ClassA/*|/*/*/ClassB/*"`
    Combine filters (AND): `dotnet test -- --treenode-filter "/*/*/*/*[Category=Integration][Priority=High]"`

---

### Coding Style
- `ImplicitUsings: enable`, `Nullable: enable`
- XML documentation required (`GenerateDocumentationFile: true`)
- Namespaces: always `LightningArc.*` (NOT `Utils.*`, NOT `Abstractions.*`)
- Deleted/removed namespaces: `Results`, `Abstractions`, `Json`, `Data`, `Mappers`, `Metalama` (these were shortened prefixes)
- Use `TryGetValue`/`TryGetError` for safe Result access (not `.Value`/`.Error`)
- Prefer `GetConnectionAsync()` in async methods; analyzer LARC020 warns on sync version

### Result Pattern
```csharp
// Safe access (preferred)
if (result.TryGetValue(out var value)) { /* use value */ }
if (result.TryGetError(out var error)) { /* handle error */ }

// Chaining
return Email.Create(email).Bind(e => _repo.AddAsync(e)).Map(u => u.ToDto());

// Multiple validation errors
Error? errors = null;
if (invalid) errors += Error.Validation.MissingField("field");
return errors != null ? errors : Result.Success();
```

---

## Key Files
| File | Purpose |
|------|---------|
| `LightningArc.Framework.slnx` | Solution file |
| `Directory.Packages.props` | Central package versions |
| `Directory.Build.props` | Root build/license metadata |
| `src/Directory.Build.props` | Source-level build props + analyzer reference |
| `publish.ps1` | Build & pack script |
| `README.md` | Project entry point |
| `docs/README.md` | Documentation index |
| `.github/workflows/ci-cd.yml` | CI/CD pipeline |

## Guidelines for Edits
1. Always read the file before editing
2. Present a detailed plan before implementing changes
3. Do not create files unless necessary — prefer editing existing files
4. Commits should follow conventional commits and be atomic and focused
5. Never auto-commit without user request

## Notes for Multi-Agent Workflows

- When dispatching test-running tasks to a subagent, restate the `--project` rule explicitly in the dispatch prompt (e.g., "every `dotnet test` command must use `--project` when targeting a project file — do not use `--filter`; use TUnit tree-node filters only"). This rule is easy for subagents to miss otherwise.