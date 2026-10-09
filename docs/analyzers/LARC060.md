# LARC060 — Missing .ToEndpointResult() in Minimal API

**Severity:** Warning  **Domain:** Web  **Code fix:** No

## What it detects

A Minimal API `MapGet`/`MapPost`/`MapPut`/`MapDelete`/`MapPatch`/`MapMethods` handler lambda returning `Result`/`Result<T>` without calling `.ToEndpointResult()`. Without it the `Result` is default-JSON-serialized instead of being mapped to the correct HTTP status code.

## ❌ Bad

```csharp
app.MapGet("/users/{id}", (Guid id) => GetUser(id)); // warns: Result serialized as JSON
```

## ✅ Good

```csharp
app.MapGet("/users/{id}", (Guid id) => GetUser(id).ToEndpointResult()); // no warning
```

## Code fix

None yet. A future fix may append `.ToEndpointResult()` to the lambda's returned expression (one edit for expression-bodied lambdas, one edit per `return` for block-bodied ones — see GAP-11).

## Suppression

```csharp
#pragma warning disable LARC060
app.MapGet("/users/{id}", (Guid id) => GetUser(id));
#pragma warning restore LARC060
```

```ini
# .editorconfig
dotnet_diagnostic.LARC060.severity = none
```

## Scope & limitations

Only lambdas passed as arguments to the six `Map*` methods (resolved on `IEndpointRouteBuilder`/`EndpointRouteBuilderExtensions`) are examined. Lambdas already returning `IResult`/`EndpointResult` (or any `IResult` implementer, including via base types) are exempt, as is any lambda whose text already contains a `.ToEndpointResult(` call.

## Related

- [LARC006](LARC006.md) — redundant try-catch for Result mapping (`ResultExceptionHandler` middleware)
- [LARC001](LARC001.md) — unsafe `.Value` access
