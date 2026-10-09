# Introduction to LightningArc

**LightningArc** is a modular, production-ready C# framework for building consistent applications by combining structured error handling, DDD primitives, and seamless ASP.NET Core integration.

It enforces best practices at compile time through Roslyn analyzers, helping developers write safer and more predictable code without relying on runtime conventions.

---

## Ecosystem Architecture

The ecosystem is divided into three major categories:

### 1. Core (Foundation)
Framework-agnostic libraries that define the base patterns.
- **`LightningArc.Results`**: Full implementation of the Result pattern.
- **`LightningArc.Primitives`**: Contracts, Value Objects, and base types.
- **`LightningArc.Json`**: Utilities for modern JSON serialization.
- **`LightningArc.Framework`**: Meta package with all core layer packages.

### 2. Data & Web (Integrations)
Bridges to popular industry frameworks.
- **`LightningArc.AspNetCore`**: Core ASP.NET Core utilities, HTTP mapping, CORS, OpenAPI.
- **`LightningArc.Data.*`**: Implementations for ADO.NET, Dapper, and Entity Framework Core.
- **`LightningArc.Mappers.*`**: Adapters for AutoMapper and Mapster.

### 3. Analyzers & Meta (Static Analysis & AOP)
Compile-time tools.
- **`LightningArc.Analyzers`**: 9 Roslyn analyzer rules and 2 code fixes for Result, ValueObjects, and Data.
- **`LightningArc.Metalama`**: Metalama-powered AOP factories.
- **`LightningArc.Metalama.Results`**: Result-related Metalama utilities.

---

## Design Philosophy

1. **Fail-Fast and Functional**: Prefer `Result` over exceptions for normal business flow.
2. **Extensible**: Most classes are designed to be inherited or extended (protecting internal APIs with `internal` and exposing what's needed with `protected`).
3. **Modern**: Leverages the latest C# features (14+) and targets modern .NET versions.
4. **Ergonomic**: Implicit operators, deconstruction, and logical operators make the library feel native to the language.

---

## What's Next?
- Learn how to handle errors with the [Result Pattern](../core-features/result-pattern.md).
- See how to simplify your controllers with [ASP.NET Core Integration](../web-integration/results-mapping.md).
- Explore [Data Access](../data-access/abstractions.md) patterns.