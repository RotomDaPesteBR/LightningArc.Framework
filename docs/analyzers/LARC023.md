# LARC023 — ValueObject should be a record

**Severity:** Warning  **Domain:** ValueObject  **Code fix:** Yes: converts the `class` to a `record`

## What it detects

A ValueObject (a type implementing `IValueObject`/`IValueObject<T>` or inheriting a known ValueObject type) declared as a `class`. ValueObjects need value-based equality, which records provide by default.

## ❌ Bad

```csharp
public class Money : IValueObject<decimal> // warns: should be a record
{
    public decimal Value { get; }
}
```

## ✅ Good

```csharp
public record Money : IValueObject<decimal>
{
    public decimal Value { get; }
}
```

## Code fix

Rewrites `class` to `record`. Per GAP-8, guards will be added so the fix is skipped when the type declares an `Equals`/`GetHashCode` override, `==`/`!=` operators, or a user-written constructor body — cases where the swap would change equality semantics.

## Suppression

```csharp
#pragma warning disable LARC023
public class Money : IValueObject<decimal> { ... }
#pragma warning restore LARC023
```

```ini
# .editorconfig
dotnet_diagnostic.LARC023.severity = none
```

## Scope & limitations

Only `class` declarations are examined; anything already a `record` is out of scope. Recognition is structural (any `IValueObject` implementer, not just built-in types). The current fix is a blind class→record swap — review the type for hand-written equality members before applying it (see GAP-8).

## Related

- [LARC020](LARC020.md) — implicit string to ValueObject conversion
- [LARC021](LARC021.md) — null ValueObject conversion to string
