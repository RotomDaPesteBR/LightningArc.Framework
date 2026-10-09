# LARC043 — ReleaseConnection called with null argument

**Severity:** Warning  **Domain:** Data  **Code fix:** No

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

None yet. A future fix may replace the null/default argument with the single local assigned from `GetConnection()`/`await GetConnectionAsync(...)` in the enclosing method — offered only when exactly one such candidate exists (see GAP-11).

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
