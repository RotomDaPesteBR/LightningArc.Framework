# LARC006 — Redundant try-catch for Result mapping

**Severity:** Warning  **Domain:** Result  **Code fix:** Yes: removes the try/catch and splices the try body in its place

## What it detects

A `try` with a single broad `catch` (`catch (Exception)` or bare `catch`) whose body contains exactly one statement: a `return` of an `Error`/`Result` factory (e.g. `Error.Application.Internal()`, `Result.Failure(...)`) — or any `Error`/`Result`-typed expression, such as a cached error variable (`return cachedError;`). In a compilation that references `LightningArc.Results.AspNetCore.ResultExceptionHandler`, the global middleware already maps unhandled exceptions to standardized errors, so this local mapping is redundant.

## ❌ Bad

```csharp
try
{
    return GetUser(id);
}
catch (Exception)
{
    return Error.Application.Internal(); // warns: middleware already does this
}
```

## ✅ Good

```csharp
return GetUser(id); // unhandled exceptions reach ResultExceptionHandler
```

## Code fix

Removes the try/catch and splices the `try` body statements in order, preserving leading trivia/comments and re-formatting indentation.

## Suppression

```csharp
#pragma warning disable LARC006
try { return Process(items); }
catch (Exception) { return Error.Application.Internal(); }
#pragma warning restore LARC006
```

```ini
# .editorconfig
dotnet_diagnostic.LARC006.severity = none
```

Suppress — don't restructure — when the try/catch provides per-item fault isolation the middleware cannot: e.g. a catch around one `Task.WhenAll` branch, or around `ResultAggregator.CheckAll` checks, where one failing item must not abort the others. (`LightningArc.Results` itself never fires this rule: the gate below is not satisfied there.)

## Scope & limitations

The rule is **only active in compilations that reference `LightningArc.Results.AspNetCore.ResultExceptionHandler`** (resolved by fully-qualified metadata name). Non-web apps, background services, and library code below the web layer are therefore silent by design. The shape must be exact: exactly one catch, a broad `System.Exception` (or no) declaration, and exactly one statement in the catch block returning an `Error`/`Result`-typed expression (a factory in the `LightningArc.Results` namespace, or any other `Error`/`Result`-typed expression such as a cached variable). Narrow catches (`catch (SqlException)`), multi-statement catch bodies, and logging inside the catch (`return LogAndReturn(ex)`) do not trigger it.

## Related

- [LARC001](LARC001.md) — unsafe `.Value` access
- [LARC007](LARC007.md) — `ResultAggregator` built with no checks
