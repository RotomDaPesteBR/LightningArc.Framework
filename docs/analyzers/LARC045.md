# LARC045 — Repository never uses Transaction

**Severity:** Info  **Domain:** Data  **Code fix:** No

## What it detects

A non-abstract `RepositoryBase`-derived class that makes at least one database-operation call (Dapper or `DbConnection`, same predicate as [LARC041](LARC041.md)) but never references the `Transaction` property anywhere in the type. Such a repository can never participate in unit-of-work mode: every operation runs outside the ambient transaction.

## ❌ Bad

```csharp
public class OrderRepository : RepositoryBase
{
    public async Task<Order?> GetAsync(IDbConnection conn, Guid id)
    {
        return await conn.QuerySingleOrDefaultAsync<Order>(sql); // warns on OrderRepository: Transaction is never used
    }
}
```

## ✅ Good

```csharp
public class OrderRepository : RepositoryBase
{
    public async Task<Order?> GetAsync(IDbConnection conn, Guid id)
    {
        return await conn.QuerySingleOrDefaultAsync<Order>(sql, transaction: Transaction); // no warning
    }
}
```

## Code fix

None is offered: the fix is a design decision (thread `transaction: Transaction` through every database-operation call, the same fix [LARC041](LARC041.md) offers per call), not a single safe edit.

## Suppression

```csharp
#pragma warning disable LARC045
public class OrderRepository : RepositoryBase
#pragma warning restore LARC045
```

```ini
# .editorconfig
dotnet_diagnostic.LARC045.severity = none
```

## Scope & limitations

Only non-abstract classes deriving from `RepositoryBase` are examined; abstract classes and classes with no database-operation calls are skipped. A single `Transaction` reference anywhere in the type satisfies the rule — this is a presence check only, the same dual self-managed/unit-of-work model as [LARC041](LARC041.md). For why `ReleaseConnection` (not `using`) is the one correct disposal path in both modes, see [LARC044](LARC044.md).

## Related

- [LARC041](LARC041.md) — missing transaction in repository call (per-call version of this check)
- [LARC044](LARC044.md) — missing `ReleaseConnection` after `GetConnection[Async]` (dual-mode rationale)
- [LARC040](LARC040.md) — sync `GetConnection` in async method
