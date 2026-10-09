# LARC020 — Implicit conversion from string to ValueObject

**Severity:** Warning  **Domain:** ValueObject  **Code fix:** Yes

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

Available (single action). The fix wraps the literal as `<VO>.Create("literal")`:

```csharp
Email email = Email.Create("not-an-email"); // fixed: explicit validation call
```

It applies to all three flagged shapes (variable declarator, assignment, call argument), resolving the destination ValueObject type the same way the analyzer does. A `TryCreate`-based reshape is deliberately out of scope — that would restructure the surrounding statement and remains a separate backlog item.

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
