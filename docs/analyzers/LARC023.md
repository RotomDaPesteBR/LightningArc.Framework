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

Rewrites `class` to `record`. The fix is withheld when the type hand-writes equality or construction semantics — an `Equals`/`GetHashCode` override, an `==`/`!=` operator, or a constructor with a body (block or `=>`) — where the swap would change runtime behavior. A plain type with none of these still offers the fix.

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

Only `class` declarations are examined; anything already a `record` is out of scope. Recognition is structural (any `IValueObject` implementer, not just built-in types). The fix declines hand-written equality/construction cases (see Code fix); otherwise review the type before applying it.

## Related

- [LARC020](LARC020.md) — implicit string to ValueObject conversion
- [LARC021](LARC021.md) — null ValueObject conversion to string
