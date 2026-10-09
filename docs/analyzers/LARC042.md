# LARC042 — Direct DbConnection instantiation in repository

**Severity:** Warning  **Domain:** Data  **Code fix:** No

## What it detects

A `new <DbConnection subtype>(...)` (any type deriving from `System.Data.Common.DbConnection`/`IDbConnection`) inside a `RepositoryBase`-derived class. Repositories must acquire connections via `GetConnection()`/`GetConnectionAsync()` so connection management stays centralized.

## ❌ Bad

```csharp
public class UserRepository : RepositoryBase
{
    public User Get(Guid id)
    {
        using var conn = new SqlConnection(_connectionString); // warns: bypasses factory
        ...
    }
}
```

## ✅ Good

```csharp
public class UserRepository : RepositoryBase
{
    public User Get(Guid id)
    {
        var conn = GetConnection(); // centralized: no warning
        try { ... }
        finally { ReleaseConnection(conn); }
    }
}
```

## Code fix

None is offered deliberately: replacing `new SqlConnection(cs)` with a factory call would silently change which database is used — only the author knows the right connection string/factory (see GAP-11 "deliberately no fix" list).

## Suppression

```csharp
#pragma warning disable LARC042
using var conn = new SqlConnection(_connectionString);
#pragma warning restore LARC042
```

```ini
# .editorconfig
dotnet_diagnostic.LARC042.severity = none
```

## Scope & limitations

Only object-creation expressions whose type walks up to `DbConnection`/`IDbConnection` count, and only inside a class that symbol-derives from `RepositoryBase`. Instantiation anywhere else (tests, factories, composition roots) is out of scope.

## Related

- [LARC040](LARC040.md) — sync `GetConnection` in async method
- [LARC044](LARC044.md) — missing `ReleaseConnection` after `GetConnection[Async]`
