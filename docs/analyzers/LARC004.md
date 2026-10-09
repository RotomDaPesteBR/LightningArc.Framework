# LARC004 — Result.Success(null) for non-nullable type

**Severity:** Warning  **Domain:** Result  **Code fix:** No

## What it detects

`Result.Success(null)` (or `Result.Ok(null)`, including `default`/`default(T)` and `null!`) where the success type `T` is a non-nullable reference type. The resulting `Result<T>` claims a non-null value but holds null, defeating nullable-reference-type guarantees downstream.

## ❌ Bad

```csharp
Result<User> GetCached() => Result.Success<User>(null); // warns: User is non-nullable
```

## ✅ Good

```csharp
Result<User?> GetCached() => Result.Success<User?>(null); // nullable T: no warning

Result<User> GetCached() => Result.Success(new User(...)); // real value: no warning
```

## Code fix

None is offered: only the author knows the real value (or whether `T` should be nullable). Supply a value or change `T` to a nullable annotation.

## Suppression

```csharp
#pragma warning disable LARC004
Result<User> r = Result.Success<User>(null);
#pragma warning restore LARC004
```

```ini
# .editorconfig
dotnet_diagnostic.LARC004.severity = none
```

## Scope & limitations

Only reference-type `T` is considered, and a nullable-annotated `T` (`User?`) is exempt. The analyzer only recognizes the literal shapes `null`, `null!`, `default`, and `default(T)` (through casts/parentheses) — a null arriving via a variable or method call is not flagged.

## Related

- [LARC001](LARC001.md) — unsafe `.Value` access
- [LARC005](LARC005.md) — shadowed `Result` error
