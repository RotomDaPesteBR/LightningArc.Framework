# Value Objects

Value Objects are immutable types that represent domain concepts defined by their attributes rather than a persistent identity.

## Available Value Objects

| Type | Namespace | Validation |
|------|-----------|------------|
| `Email` | `LightningArc.Primitives.ValueObjects` | RFC 5322 regex |
| `Cpf` | `LightningArc.Primitives.ValueObjects` | 11-digit Brazilian CPF checksum |
| `Cnpj` | `LightningArc.Primitives.ValueObjects` | 14-digit Brazilian CNPJ checksum |
| `PhoneNumber` | `LightningArc.Primitives.ValueObjects` | E.164 format |
| `Url` | `LightningArc.Primitives.ValueObjects` | Absolute URL with scheme |

## Common Features
- **Immutability**: Once created, the value cannot be changed.
- **Equality**: Two instances are equal if their values are identical (C# records).
- **`TryCreate`**: Safe factory that returns `bool` instead of throwing.
- **Implicit string conversion**: Supported but flagged by LARC010 analyzer.

### Safe Creation (Recommended)
```csharp
// TryCreate - won't throw
if (Email.TryCreate(input, out var email)) { /* use email */ }

// Create - throws on invalid
Email email = Email.Create("user@example.com");
```

### Value Objects with Result
```csharp
using LightningArc.Primitives.Results;

// String → Result<Email> (returns Failure instead of throwing)
Result<Email> result = "bad-email".CreateEmailResult();

// ValueObject → Result<Email> (for composition)
Result<Email> wrapped = email.ToResult();
```

> LARC010 warns on implicit string → ValueObject conversion. LARC011 warns on nullable ValueObject → string. Prefer `TryCreate` or `CreateXxxResult` extensions in critical paths.
```csharp
// Preferred: returns Failure instead of throwing
Result<Email> result = "bad-email".CreateEmailResult();
if (result.IsFailure) { /* reason in result.Error */ }

Email email = Email.Create("user@example.com");
Result<Email> wrapped = email.ToResult();
```

Each ValueObject has its own extensions:

| Extension | Purpose |
|---|---|
| `Create{Type}Result(this string)` | Validates and returns `Result<{Type}>` |
| `ToResult(this {Type})` | Wraps ValueObject as `Result<{Type}>` |

### All Value Objects

The full list is: `Email`, `Cpf`, `Cnpj`, `PhoneNumber`, `Url`.

⚠️ **Analyzer note**: LARC010 warns on implicit string → ValueObject conversion and LARC011 warns on nullable ValueObject → string. Prefer `TryCreate` or `Create{Type}Result` extensions.
