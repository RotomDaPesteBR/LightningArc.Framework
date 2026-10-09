# LARC001 — Unsafe access to Result.Value

**Severity:** Warning  **Domain:** Result  **Code fix:** Yes: wraps the access in `TryGetValue(out var value)`

## What it detects

Reads of `.Value` on a `Result`/`Result<T>` that is not proven to be a success at that point. Accessing `.Value` on a failure throws, so the analyzer requires a success guard or a `TryGetValue` call first.

## ❌ Bad

```csharp
Result<User> result = GetUser(id);
string name = result.Value.Name; // warns: may throw if result is a failure
```

## ✅ Good

```csharp
Result<User> result = GetUser(id);
if (result.IsSuccess)
{
    string name = result.Value.Name; // guarded: no warning
}

if (result.TryGetValue(out var user))
{
    string name = user.Name; // no warning
}
```

## Code fix

Rewrites the access into a `TryGetValue(out var value)` guard.

## Suppression

```csharp
#pragma warning disable LARC001
string name = result.Value.Name;
#pragma warning restore LARC001
```

```ini
# .editorconfig
dotnet_diagnostic.LARC001.severity = none
```

## Scope & limitations

No diagnostic when the access is guarded: an `IsSuccess` check in an `if`/ternary/`&&`/`||`, a `TryGetValue` call, or a preceding early exit on `IsFailure` (`return`/`throw`/`continue`/`break`). Accesses inside conversion operators and accesses with the null-forgiving operator (`result.Value!`) are also exempt. Note the widened receiver check: the rule matches non-generic `Result` too, but non-generic `Result` has no `.Value` member, so such code already fails to compile for an unrelated reason.

## Related

- [LARC002](LARC002.md) — unsafe `.Error` access
- [LARC003](LARC003.md) — discarded `Result`
- [LARC005](LARC005.md) — shadowed `Result` error
