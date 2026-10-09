# LARC021 — Potential null ValueObject conversion to string

**Severity:** Warning  **Domain:** ValueObject  **Code fix:** Yes: rewrites the expression to `expr?.Value ?? string.Empty`

## What it detects

A nullable-annotated ValueObject used where a `string` is expected — in a `string` variable declarator, a `string` assignment target, or a `string` call parameter. The implicit conversion to string can throw on null.

## ❌ Bad

```csharp
Email? maybeEmail = GetEmail();
string s = maybeEmail; // warns: may throw if null
```

## ✅ Good

```csharp
Email? maybeEmail = GetEmail();
string s = maybeEmail?.Value ?? string.Empty; // no warning
```

## Code fix

Rewrites each of the three call-site shapes (declarator initializer, assignment right-hand side, call argument) to `expr?.Value ?? string.Empty`. The fix assumes every ValueObject exposes a string `Value` — verified against the `LightningArc.Primitives` types.

## Suppression

```csharp
#pragma warning disable LARC021
string s = maybeEmail;
#pragma warning restore LARC021
```

```ini
# .editorconfig
dotnet_diagnostic.LARC021.severity = none
```

## Scope & limitations

Only nullable-annotated ValueObjects trigger the rule; a non-nullable-annotated ValueObject is exempt. Like [LARC020](LARC020.md), recognition is structural via `ValueObjectTypeRecognizer`.

## Related

- [LARC020](LARC020.md) — implicit string to ValueObject conversion
- [LARC022](LARC022.md) — discarded ValueObject creation result
