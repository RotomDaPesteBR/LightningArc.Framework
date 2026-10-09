# LARC044 — Missing ReleaseConnection after GetConnection

**Severity:** Warning  **Domain:** Data  **Code fix:** No

## What it detects

A `GetConnection()`/`GetConnectionAsync()` call with no matching `ReleaseConnection(...)` in a `finally` block of the same method. An unreleased factory-acquired connection leaks when the repository owns its connection.

## ❌ Bad

```csharp
public User Get(Guid id)
{
    var conn = GetConnection(); // warns: never released
    return conn.QuerySingle<User>(sql);
}
```

## ✅ Good

```csharp
public User Get(Guid id)
{
    var conn = GetConnection();
    try
    {
        return conn.QuerySingle<User>(sql);
    }
    finally
    {
        ReleaseConnection(conn); // no warning
    }
}
```

## Code fix

None is offered deliberately: restructuring an arbitrary method body into try/finally cannot be done safely by a machine (see GAP-11 "deliberately no fix" list).

## Suppression

```csharp
#pragma warning disable LARC044
var conn = GetConnection();
#pragma warning restore LARC044
```

```ini
# .editorconfig
dotnet_diagnostic.LARC044.severity = none
```

## Dual-mode rationale: why ReleaseConnection, not `using`

`RepositoryBase` supports two modes: self-managed (it creates the connection via the factory and owns it) and unit-of-work (constructed with an external connection/transaction it does not own). `ReleaseConnection` **no-ops when the connection came from an externally-provided `DbConnection` or an active `DbTransaction`**, so the repository never closes a connection it doesn't own. A bare `using` on the acquired connection would close it unconditionally, breaking unit-of-work mode. `ReleaseConnection` in a `finally` is therefore the one correct disposal path in both modes — which is why this rule flags its absence with high confidence instead of treating `using` as an acceptable alternative. ([LARC041](LARC041.md) and [LARC043](LARC043.md) share this model.)

## Scope & limitations

A directly-returned acquisition (`return GetConnection(...);`, `=> GetConnection(...)`, optionally awaited) is treated as forwarding to the caller's ownership and skipped — this keeps `RepositoryBase`'s own forwarding overloads quiet. Only regular method bodies are examined (constructors, field initializers, and local functions are out of scope). Acquire-in-one-method/release-in-another false-positives by design: the check is purely syntactic within one method and never does call-graph analysis. A `finally` calling an unrelated same-named `ReleaseConnection` does not satisfy the rule (the call must resolve to `RepositoryBase.ReleaseConnection`).

## Related

- [LARC043](LARC043.md) — `ReleaseConnection(null)`
- [LARC041](LARC041.md) — missing transaction in repository call
- [LARC040](LARC040.md) — sync `GetConnection` in async method
