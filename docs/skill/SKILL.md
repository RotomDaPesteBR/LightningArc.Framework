---
name: lightning-arc-utils
description: Comprehensive documentation skill for the LightningArc.Utils C# library ecosystem. Use for Result patterns, Domain Value Objects, and ASP.NET Core integration.
---

# LightningArc.Utils Documentation Skill

This skill contains the complete documentation for `LightningArc.Utils`, including conceptual guides, architectural patterns, and API references.

## Directory Structure

| Directory | Contents |
|-----------|----------|
| `Skill/content/` | Conceptual documentation (Result Pattern, Value Objects, Web Integration, Metalama, Data Access) |
| `Skill/code/` | Reference implementations and sample code. |
| `API/` | Detailed API reference Markdown files (per file mirror) |
| `core-features/`, `data-access/`, `web-integration/`, `advanced/` | High-level usage guides and examples |

## Finding Information

1. **Search by Keywords**: Use `grep` to find specific patterns or classes in the documentation:
   ```bash
   grep -r "Match" docs/API/
   grep -r "Error.Validation" docs/API/
   ```
2. **Browse Conceptual Docs**: Start with `docs/core-features/result-pattern.md` for the core library logic.
3. **Reference Code**: Check `docs/Skill/code/SampleService.cs` for a complete end-to-end example of the patterns.

## Key Entry Points

| Topic | File | Description |
|-------|------|-------------|
| **Result Pattern** | `docs/core-features/result-pattern.md` | Core functional logic for success/failure handling. |
| **Value Objects** | `docs/core-features/value-objects.md` | Immutable domain types (e.g., Email). |
| **Web Integration** | `docs/web-integration/results-mapping.md` | Mapping Results to HTTP responses. |
| **Data & Mappers** | `docs/data-access/abstractions.md` | Repositories, UoW, and Mapping abstractions. |
| **Metalama** | `docs/advanced/metalama.md` | AOP extensions and code generation. |

## Common Patterns

### Functional Pipeline
Always chain operations to maintain a clear flow of data and error propagation.

```csharp
return await GetProduct(id)
    .Ensure(p => p.Stock > 0, Error.Resource.Unavailable("Out of stock"))
    .Map(p => p.Price * taxRate)
    .Match(
        success => Result.Success(success.Value),
        error => Result.Failure(error)
    );
```

### Returning Results from Web Endpoints
Leverage implicit conversion to `EndpointResult` for clean APIs.

```csharp
app.MapDelete("/resource/{id}", (int id) => 
    service.Delete(id)); // Automatically returns 204 or 404
```

## Best Practices

| Rule | Description |
|------|-------------|
| **No Exceptions** | Use `Result.Failure(error)` for expected failures. |
| **Always Match** | Use the `Match` method at the end of your chain to ensure all paths are handled. |
| **Typed Errors** | Use nested modules in `Error` (Validation, Resource, etc.). |
| **Async First** | Prefer `TaskResult<T>` and async extension methods. |

## External Resources
*   **GitHub**: https://github.com/LightningArc/Utils
*   **Docs Root**: docs/getting-started/introduction.md

