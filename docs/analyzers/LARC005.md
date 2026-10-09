# LARC005 — Result error is shadowed

**Severity:** Warning  **Domain:** Result  **Code fix:** No

## What it detects

A `return` (or arrow-expression body) that produces a *new* `Error`/`Result.Failure(...)` from inside an `if (x.IsFailure)` / `if (!x.IsSuccess)` guard without ever consuming `x.Error`. The original error — and its traceability — is silently replaced.

## ❌ Bad

```csharp
if (result.IsFailure)
{
    return Error.Validation.InvalidFormat("id"); // warns: result.Error never consumed
}
```

## ✅ Good

```csharp
if (result.IsFailure)
{
    Log(result.Error);
    return result.Error + Error.Validation.InvalidFormat("id"); // consumed: no warning
}
```

## Code fix

None yet. A future fix may rewrite `return <newError>;` to `return <guard>.Error + <newError>;` (or `Error.Aggregate(...)`), reusing the guard-resolution logic already in the analyzer (see GAP-11).

## Suppression

```csharp
#pragma warning disable LARC005
if (result.IsFailure)
{
    return Error.Validation.InvalidFormat("id");
}
#pragma warning restore LARC005
```

```ini
# .editorconfig
dotnet_diagnostic.LARC005.severity = none
```

## Scope & limitations

The guard must be the `x.IsFailure` / `!x.IsSuccess` shape on an enclosing `if`; other guard shapes (switch, ternaries) are not recognized. "Consumed" means any `x.Error` access (or any use of `x` itself when `x` is already an `Error`) anywhere else in the enclosing block. Only `Error`/`Result`-typed factory invocations and `new Error(...)` count as "new error" returns.

## Related

- [LARC002](LARC002.md) — unsafe `.Error` access
- [LARC008](LARC008.md) — `Error` accumulated via `+=` in a loop
