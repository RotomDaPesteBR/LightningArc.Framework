# LARC005 — Result error is shadowed

**Severity:** Warning  **Domain:** Result  **Code fix:** Yes

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

Available. The fix rewrites `return <newError>;` to `return <guard>.Error + <newError>;`, using the `Error` `+` accumulation idiom (a guard that already *is* an `Error` is combined bare, without `.Error`). The guard is resolved with the same logic the analyzer uses — never re-derived. Withholding conditions (no fix is offered): when no failure guard resolves at the return, or when the returned expression is itself a `Result` (e.g. `Result.Failure(...)`) rather than an `Error`, since `Error + Result` has no operator overload.

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
