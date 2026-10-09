# LARC022 — ValueObject creation result is discarded

**Severity:** Info  **Domain:** ValueObject  **Code fix:** Yes: prefixes the statement with `_ = `

## What it detects

A `Create(...)`/`TryCreate(...)` call on a ValueObject type used as a bare statement. The validated value (or validation failure) is thrown away, so the validation never takes effect.

## ❌ Bad

```csharp
Email.Create("not-an-email"); // warns: result discarded, validation lost
```

## ✅ Good

```csharp
Result<Email> result = Email.Create("not-an-email");
if (result.TryGetValue(out var email))
{
    // use email
}
```

Explicit discard (when validation is intentionally ignored):

```csharp
_ = Email.Create("not-an-email"); // no warning
```

## Code fix

Inserts an explicit discard: `Email.Create("a@b.c");` becomes `_ = Email.Create("a@b.c");`.

## Suppression

```csharp
#pragma warning disable LARC022
Email.Create("a@b.c");
#pragma warning restore LARC022
```

```ini
# .editorconfig
dotnet_diagnostic.LARC022.severity = none
```

## Scope & limitations

Only `Create`/`TryCreate` methods whose containing type is structurally a ValueObject (`ValueObjectTypeRecognizer`) trigger the rule. Like [LARC003](LARC003.md), there are no recognized-safe exemptions — write the discard explicitly if it is deliberate.

## Related

- [LARC003](LARC003.md) — discarded `Result`
- [LARC020](LARC020.md) — implicit string to ValueObject conversion
