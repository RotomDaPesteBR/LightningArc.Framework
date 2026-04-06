# Gemini Context: Utils

This `GEMINI.md` file provides context for AI agents interacting with the `Utils` project, a C# .NET library ecosystem.

## Project Overview

**Utils** (Namespace: `LightningArc.Core`) is a collection of C# utilities and patterns designed to promote clean, expressive, and robust code in .NET applications.

*   **Primary Focus:** Functional Result Pattern (Error Handling), Immutable Value Objects, and ASP.NET Core Integration.
*   **Technologies:** .NET (Multi-targeted: `netstandard2.0`, `net6.0`, `net9.0`, `net10.0`), ASP.NET Core, Metalama (AOP).
*   **Architecture:** Modular design separated into Core, Web, and Meta layers.

## Documentation

The project maintains extensive documentation in the `Docs/` directory. **Always consult these files before answering questions about usage or implementation.**

*   **[Usage Guides](Docs/Usage/README.md)**: Public API and examples.
*   **[Implementation](Docs/Implementation/README.md)**: Internal logic and file-by-file analysis.

## Architecture & Modules

The solution is organized into logical folders:

*   **`Core/`**: The foundation of the library.
    *   `Utils.Results`: Implements the `Result<T>` pattern (Either monad).
    *   `Utils.Abstractions`: Value Objects (e.g., `Email`).
    *   `Utils.Json`: JSON serialization helpers.
*   **`Web/`**: Extensions for web applications.
    *   `Utils.AspNet`: Metapackage bundle.
    *   `Utils.AspNet.Results`: Bridges `Result<T>` to `IResult` (HTTP Responses).
    *   `Utils.AspNet.OpenAPI`: OpenAPI/Swagger extensions.
    *   `Utils.AspNet.CORS`: CORS policies helpers.
*   **`Meta/`**: Meta-programming and Aspect-Oriented Programming (AOP).
    *   `Utils.Metalama`: Metapackage for aspects.
    *   `Utils.Metalama.Extensions`: Extensions for `IAspectBuilder`.
    *   `Utils.Metalama.Factories`: Factories for `IType`.

## Building and Running

This project uses standard .NET CLI tools and a custom PowerShell script for packaging.

### Key Commands

*   **Build Solution:** `dotnet build`
*   **Run Tests:** `dotnet test`
*   **Create NuGet Packages:** `.\publish.ps1`
    *   **Output:** `Publish\Packages\<Version>\`
    *   **Configuration:** Reads version from `Directory.Packages.props`.

## Development Conventions

*   **Package Management:** Uses **Central Package Management** (`Directory.Packages.props`).
*   **Versioning:** Centrally managed in `Directory.Packages.props`.
*   **Coding Style:**
    *   `ImplicitUsings`: Enabled.
    *   `Nullable`: Enabled (Strict null safety).
    *   **Documentation:** XML documentation is required.
*   **Namespaces:**
    *   Core: `LightningArc.Core.*`
    *   Web: `LightningArc.AspNetCore.*`
    *   Meta: `LightningArc.Core.Metalama.*`

## Key Files

*   `Utils.slnx`: The .NET Solution file.
*   `Directory.Packages.props`: Central versioning.
*   `publish.ps1`: Automation script.
*   `README.md`: Project entry point.
*   `Docs/README.md`: Documentation entry point.

## Guide Lines

*   Always read the file before start editing
*   Always present a detailed summary of the plan before you begin implementing the requested changes.
