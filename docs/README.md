# LightningArc Documentation

Welcome to the official documentation of the **LightningArc** ecosystem. This library is designed to provide robust architectural patterns and high-performance utilities for modern .NET applications.

## Quick Start
- **[Introduction](getting-started/introduction.md)**: Overview and project philosophy.
- **[Result Pattern](core-features/result-pattern.md)**: Functional error handling done right.
- **[Web Integration](web-integration/results-mapping.md)**: Automating HTTP responses in ASP.NET Core.

---

## Sections

### 1. [Core Features](core-features/result-pattern.md)
The heart of the ecosystem.
- [Result Pattern](core-features/result-pattern.md): Successes, Failures, and Error Aggregation.
- [Value Objects](core-features/value-objects.md): Primitives with business rules (Email, CPF, CNPJ, etc).
- [JSON Serialization](core-features/json-serialization.md): Converters and extensions for `System.Text.Json`.

### 2. [Data Access](data-access/abstractions.md)
Standardizing the persistence layer.
- [Abstractions](data-access/abstractions.md): Repositories and Unit of Work.
- [ADO.NET & Dapper](data-access/ado-dapper.md): Lightweight implementations for SQL Server and Oracle.
- [Entity Framework Core](data-access/entity-framework.md): Base repository for EF.
- [Mappers](data-access/mappers.md): Adapters for AutoMapper and Mapster.

### 3. [Web Integration](web-integration/results-mapping.md)
ASP.NET Core specific extensions.
- [HTTP Mapping](web-integration/results-mapping.md): Automatic `Result` to RFC 7807 (Problem Details) conversion.
- [OpenAPI](web-integration/openapi.md): Enhanced Swagger and API documentation support.

### 4. [Analyzers](analyzers/README.md)
Compile-time static analysis for the ecosystem.
- [Rules](analyzers/README.md): 9 rules and 2 code fixes for Result, ValueObjects, and Data.

### 5. [Advanced](advanced/internals/README.md)
Internal implementation details and architecture decisions.
- [Internals](advanced/internals/README.md): Source-level documentation of core components.

---

## Package Overview

* **`LightningArc.Framework`** → Meta package for the full framework.
* **`LightningArc.AspNetCore`** → ASP.NET Core integrations (includes `Results.AspNetCore`).
* **`LightningArc.Results`** → Core Result pattern (Success/Failure, Error, Value Objects).
* **`LightningArc.Primitives`** → Base types and abstractions.
* **`LightningArc.Json`** → `System.Text.Json` converters for Value Objects.
* **`LightningArc.Analyzers`** → Roslyn analyzers enforcing best practices at compile time.
* **`LightningArc.Metalama`** → Aspect-oriented programming integration.

---

## Philosophy

* **Fail-Fast and Functional**: Prefer `Result` over exceptions for normal business flow.
* **Compile-Time Enforcement**: Roslyn analyzers catch misuse before runtime.
* **Extensible**: Most classes are designed for inheritance and extension.
* **Lightweight**: Minimal dependencies, optimized for high-throughput APIs.

---

## Getting Started

Install the meta package:

```bash
dotnet add package LightningArc.Framework
```

For ASP.NET Core integration:

```bash
dotnet add package LightningArc.AspNetCore
```

Or install individual modules directly:

```bash
dotnet add package LightningArc.Results
dotnet add package LightningArc.Results.AspNetCore
dotnet add package LightningArc.Primitives
```