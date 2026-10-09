# LARC020 — Implicit conversion from string to ValueObject

**Severity:** Warning  **Domain:** ValueObject  **Code fix:** No

## What it detects

A string literal implicitly converted to a ValueObject (`Email`, `Cpf`, `Cnpj`, `PhoneNumber`, `Url`, or any `IValueObject`/`IValueObject<T>`) in a variable declarator, an assignment, or a call argument. The implicit conversion can throw on invalid input; prefer explicit `Create`/`TryCreate` validation.

## ❌ Bad

```csharp
Email email = "not-an-email"; // warns: implicit conversion may throw
```

## ✅ Good

```csharp
Result<Email> result = Email.Create("not-an-email");
if (result.TryGetValue(out var email))
{
    // use email
}
```

## Code fix

None yet. A future fix may wrap the literal as `<VO>.Create("literal")`, optionally with a second action using `TryCreate(..., out var x)` (see GAP-11).

## Suppression

```csharp
#pragma warning disable LARC020
Email email = "admin@example.com";
#pragma warning restore LARC020
```

```ini
# .editorconfig
dotnet_diagnostic.LARC020.severity = none
```

## Scope & limitations

Only string *literals* are examined — a string arriving via a variable or method call is not flagged. Arguments passed by `ref`/`out` are skipped. Recognition is structural via `ValueObjectTypeRecognizer` (built-in primitives types plus any consumer type implementing `IValueObject`/`IValueObject<T>`).

## Related

- [LARC021](LARC021.md) — null ValueObject conversion to string
- [LARC022](LARC022.md) — discarded ValueObject creation result
- [LARC023](LARC023.md) — ValueObject should be a record
