# LARC003 — Result value is discarded

**Severity:** Info  **Domain:** Result  **Code fix:** Yes: prefixes the statement with `_ = `

## What it detects

An expression-statement invocation whose return value is a `Result`/`Result<T>` and is silently dropped. A discarded `Result` means a potential failure is never observed.

## ❌ Bad

```csharp
SaveUser(user); // warns: returns Result<User>, discarded
```

## ✅ Good

```csharp
Result<User> result = SaveUser(user);
if (result.IsFailure)
{
    return result.Error;
}
```

Explicit discard (when ignoring the result is deliberate):

```csharp
_ = SaveUser(user); // no warning
```

## Code fix

Inserts an explicit discard: `SaveUser(user);` becomes `_ = SaveUser(user);`.

## Suppression

```csharp
#pragma warning disable LARC003
SaveUser(user);
#pragma warning restore LARC003
```

```ini
# .editorconfig
dotnet_diagnostic.LARC003.severity = none
```

## Scope & limitations

Only fires on invocation expression statements whose return type resolves to `LightningArc.Results.Result`/`Result<T>`. Types that merely share the name (e.g. a consumer's `SearchResult`/`ApiResult`) do not trigger it. There are no recognized-safe exemptions: if the result is intentionally ignored, write the discard explicitly.

## Related

- [LARC001](LARC001.md) — unsafe `.Value` access
- [LARC022](LARC022.md) — discarded ValueObject creation result
