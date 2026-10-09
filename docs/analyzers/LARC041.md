# LARC041 — Missing transaction in repository call

**Severity:** Info  **Domain:** Data  **Code fix:** Yes: appends `transaction: Transaction`

## What it detects

A Dapper (`Query`/`QueryAsync`/`Execute`/`ExecuteAsync`/`QuerySingle*`/`QueryFirst*`) or `DbConnection` operation call inside a `RepositoryBase`-derived class that does not pass the `Transaction` property. Without it the operation runs outside the ambient unit-of-work transaction.

## ❌ Bad

```csharp
var users = await conn.QueryAsync<User>(sql); // warns: no transaction argument
```

## ✅ Good

```csharp
var users = await conn.QueryAsync<User>(sql, transaction: Transaction); // no warning
```

## Code fix

Appends `transaction: Transaction` to the call.

## Suppression

```csharp
#pragma warning disable LARC041
var users = await conn.QueryAsync<User>(sql);
#pragma warning restore LARC041
```

```ini
# .editorconfig
dotnet_diagnostic.LARC041.severity = none
```

## Scope & limitations

This is a **presence check only**: passing `transaction: Transaction` satisfies the rule even when `Transaction` is null at runtime (self-managed mode) — by design for the dual self-managed/unit-of-work model. A named `transaction:` argument or a positional argument at the transaction parameter's index both count as provided. Only calls inside a `RepositoryBase`-derived class are examined, and only methods that actually declare a transaction parameter (a `transaction` name or `DbTransaction`-typed parameter) can trigger the rule. For why `ReleaseConnection` (not `using`) is the one correct disposal path in both modes, see [LARC044](LARC044.md).

## Related

- [LARC044](LARC044.md) — missing `ReleaseConnection` after `GetConnection[Async]` (dual-mode rationale)
- [LARC040](LARC040.md) — sync `GetConnection` in async method
- [LARC043](LARC043.md) — `ReleaseConnection(null)`
