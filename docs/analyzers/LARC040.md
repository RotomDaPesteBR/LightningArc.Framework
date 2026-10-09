# LARC040 — Synchronous GetConnection in async method

**Severity:** Warning  **Domain:** Data  **Code fix:** Yes: rewrites to `await GetConnectionAsync(...).ConfigureAwait(false)`

## What it detects

A call to the synchronous `GetConnection()` inside an `async` method of a `RepositoryBase`-derived class. The sync call blocks a thread-pool thread; use the async overload.

## ❌ Bad

```csharp
public async Task<User> GetAsync(Guid id, CancellationToken ct)
{
    var conn = GetConnection(); // warns: sync call in async method
    ...
}
```

## ✅ Good

```csharp
public async Task<User> GetAsync(Guid id, CancellationToken ct)
{
    var conn = await GetConnectionAsync(ct).ConfigureAwait(false); // no warning
    ...
}
```

## Code fix

Rewrites to `await GetConnectionAsync(ct).ConfigureAwait(false)` when the enclosing method has a `CancellationToken` parameter (first match wins), or `await GetConnectionAsync().ConfigureAwait(false)` when it does not. `this.` qualification is preserved.

## Suppression

```csharp
#pragma warning disable LARC040
var conn = GetConnection();
#pragma warning restore LARC040
```

```ini
# .editorconfig
dotnet_diagnostic.LARC040.severity = none
```

## Scope & limitations

Three conditions must all hold: the call resolves to exactly `GetConnection` (not `GetConnectionAsync`), the enclosing method is `async`, and the enclosing class derives from `RepositoryBase`. Name lookalikes (`FakeRepositoryBaseForTests`, `MyRepositoryBaseClass`) do not count — recognition is symbol-based, not name-based. Calls outside a `RepositoryBase` subclass, or in non-async methods, are out of scope.

## Related

- [LARC044](LARC044.md) — missing `ReleaseConnection` after `GetConnection[Async]`
- [LARC041](LARC041.md) — missing transaction in repository call
