# LARC002 — Unsafe access to Result.Error

**Severity:** Warning  **Domain:** Result  **Code fix:** Yes: wraps the access in `TryGetError(out var error)`

## What it detects

Reads of `.Error` on a `Result`/`Result<T>` that is not proven to be a failure at that point. Accessing `.Error` on a success throws, so the analyzer requires a failure guard or a `TryGetError` call first.

## ❌ Bad

```csharp
Result<User> result = GetUser(id);
Error err = result.Error; // warns: may throw if result is a success
```

## ✅ Good

```csharp
Result<User> result = GetUser(id);
if (result.IsFailure)
{
    Error err = result.Error; // guarded: no warning
}

if (result.TryGetError(out var err))
{
    Log(err); // no warning
}
```

## Code fix

Rewrites the access into a `TryGetError(out var error)` guard.

## Suppression

```csharp
#pragma warning disable LARC002
Error err = result.Error;
#pragma warning restore LARC002
```

```ini
# .editorconfig
dotnet_diagnostic.LARC002.severity = none
```

## Scope & limitations

No diagnostic when the access is guarded: an `IsFailure` check in an `if`/ternary/`&&`/`||`, a `TryGetError` call, or a preceding early exit on `IsSuccess` (`return`/`throw`/`continue`/`break`). Accesses inside conversion operators and accesses with the null-forgiving operator (`result.Error!`) are also exempt.

## Related

- [LARC001](LARC001.md) — unsafe `.Value` access
- [LARC005](LARC005.md) — shadowed `Result` error
