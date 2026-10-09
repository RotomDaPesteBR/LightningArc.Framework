# LARC043 — ReleaseConnection called with null argument

**Severity:** Warning  **Domain:** Data  **Code fix:** Yes

## What it detects

A `ReleaseConnection(null)` (or `ReleaseConnection(default)`/`ReleaseConnection(default(...))`) call on `RepositoryBase`. Releasing null is a no-op that usually means the acquired connection variable was lost or never assigned.

## ❌ Bad

```csharp
var conn = await GetConnectionAsync(ct).ConfigureAwait(false);
try { ... }
finally { ReleaseConnection(null); } // warns: releases nothing
```

## ✅ Good

```csharp
var conn = await GetConnectionAsync(ct).ConfigureAwait(false);
try { ... }
finally { ReleaseConnection(conn); } // no warning
```

## Code fix

Available. The fix replaces the null/default argument with the single local variable assigned from `GetConnection()` / `await GetConnectionAsync(...)` in the enclosing method:

```csharp
finally { ReleaseConnection(conn); } // fixed: releases the acquired connection
```

Withholding conditions (no fix is offered): when zero such locals exist, or when more than one exists (guessing would risk releasing the wrong connection). The candidate call must produce a `DbConnection` (unwrapping `Task`/`ValueTask` for the async overload), so an unrelated same-named method returning something else never qualifies.

## Suppression

```csharp
#pragma warning disable LARC043
finally { ReleaseConnection(null); }
#pragma warning restore LARC043
```

```ini
# .editorconfig
dotnet_diagnostic.LARC043.severity = none
```

## Scope & limitations

The analyzer verifies the call targets `RepositoryBase.ReleaseConnection` by symbol (equality-with-`RepositoryBase` first, inheritance walk as fallback), so an unrelated consumer-defined `ReleaseConnection(...)` method never triggers it. Note the call fires even when written inside a `RepositoryBase`-derived class, since the method is declared on `RepositoryBase` itself. For why `ReleaseConnection` (not `using`) is the one correct disposal path in both modes, see [LARC044](LARC044.md).

## Related

- [LARC044](LARC044.md) — missing `ReleaseConnection` after `GetConnection[Async]` (dual-mode rationale)
- [LARC041](LARC041.md) — missing transaction in repository call
