# Gemini Context: LightningArc.Utils

This `GEMINI.md` provides context for AI agents working with the LightningArc C# .NET library ecosystem.

---

## Project Overview

**LightningArc** is a collection of C# utilities and patterns designed to promote clean, expressive, and robust code in .NET applications.

- **Primary Focus:** Functional Result Pattern (Error Handling), Immutable Value Objects, Repository Pattern, ASP.NET Core Integration, and Roslyn Static Analyzers
- **Technologies:** .NET (Multi-targeted: `netstandard2.0`, `net9.0`, `net10.0`), ASP.NET Core, TUnit (testing)
- **Architecture:** Modular design with Core, Data, Web, Meta, and Analyzers layers
- **License:** Business Source License 1.1 (converts to Apache 2.0 on Change Date)

---

## Architecture

### Core Layer (`src/Core/`)
Foundation types and patterns, framework-agnostic.

- **Primitives** — Value Objects: `Email`, `Cpf`, `Cnpj`, `PhoneNumber`, `Url`
- **Results** — Result pattern implementation with `TryGetValue`, `TryGetError`, chaining operators
- **Primitives.Results** — Extensions returning `Result<T>` instead of throwing exceptions
- **Json** — `System.Text.Json` converters
- **Core** — General utilities

### Data Layer (`src/Data/`)
Repository patterns and ADO.NET helpers.

- **Data.Abstractions** — Repository, Unit of Work, Connection Factory interfaces
- **Data.ADO** — Base `RepositoryBase` with `GetConnection()` / `GetConnectionAsync()`
- **Data.ADO.SqlServer**, **Data.ADO.Oracle**, **Data.ADO.SqlBuilder** — Database providers
- **Data.EntityFramework** — EF Core repository base
- **Mappers** — AutoMapper and Mapster adapters

### Web Layer (`src/Web/`)
ASP.NET Core integration.

- **AspNetCore** — Core ASP.NET Core utilities
- **Results.AspNetCore** — Maps `Result<T>` to HTTP responses (RFC 7807 Problem Details)
- **OpenAPI.AspNetCore** — Swagger/OpenAPI transformers for ValueObjects
- **CORS.AspNetCore** — Pre-configured CORS policies

### Analyzers (`src/Analyzers/`)
9 Roslyn analyzer rules + 2 automatic code fixes.
- Auto-referenced by all library projects via `src/Directory.Build.props`
- See `docs/analyzers/README.md` for rule catalog

### Meta Layer (`src/Meta/`)
Metalama-powered AOP factories.

---

## Key Conventions

### Build & Config
- **Central Package Management** — `Directory.Packages.props` (version 1.5.0)
- **Centralized build props** — `Directory.Build.props` (root) and `src/Directory.Build.props`
- **Pack on build** — `GeneratePackageOnBuild=true`
- **Artifacts** — Output to `.artifacts/` folder

### Coding Style
- `ImplicitUsings: enable`, `Nullable: enable`
- XML documentation required
- Namespaces: always `LightningArc.*` (NOT old prefixes like `Utils.*`, `Abstractions.*`)

### Value Objects
- Use `TryCreate()` for non-throwing creation
- Use `As{Type}()` for Result-based validation (e.g., `input.AsEmail()`)
- Avoid implicit string conversion (flagged by LARC010 analyzer)

### Result Pattern
- Prefer `TryGetValue`/`TryGetError` over unsafe `.Value`/`.Error` access
- Use chaining: `Bind`, `Map`, `Ensure`, `Tap`, `Match`
- Aggregate errors with `+` operator

### Data Access
- Always inherit from `RepositoryBase` for ADO.NET repositories
- Prefer `GetConnectionAsync()` in async methods (LARC020 warns on sync version)
- Use `ReleaseConnection(connection)` for cleanup (LARC030 warns on `null`)

---

## Deleted Projects (DO NOT reference)
- `CORS.AspNetCore` (deleted in commit 7d4ec95)
- `Utils.AspNetCore` metapackage (deleted)
- `Metalama` main project (deleted)

---

## Documentation

Comprehensive documentation is in the `docs/` directory. Always consult relevant files before answering questions:
- **[Usage Guides](docs/core-features/)** — Result Pattern, Value Objects
- **[Data Access](docs/data-access/)** — Repository and ADO.NET usage
- **[Web](docs/web-integration/)** — ASP.NET Core integration
- **[Analyzers](docs/analyzers/)** — Static analysis rule catalog
- **[API Reference](docs/api/)** — Types and method signatures

---

## Building and Running

- **Build:** `dotnet build`
- **Test:** `dotnet test`
- **Pack NuGet packages:** `.\publish.ps1`

---

## Key Files

- `Utils.slnx` — Solution file
- `Directory.Packages.props` — Central package versions
- `Directory.Build.props` — Root build configuration and license metadata
- `src/Directory.Build.props` — Source-level build props + analyzer references
- `publish.ps1` — Build and pack automation script
- `README.md` — Project entry point
- `docs/README.md` — Documentation index

---

## Guidelines for Edits

1. Always read the file before editing
2. Present a detailed plan before implementing changes
3. Do not create files unless necessary — prefer editing existing files
4. Commits should be atomic and focused
5. Never auto-commit without user request
