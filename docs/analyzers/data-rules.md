# Data Rules

Rules LARC040–LARC045 cover the repository connection lifecycle and transactions in `RepositoryBase`-derived classes. Each section below summarizes the rule — follow the link for the full description, examples, suppression, and limitations.

---

## LARC040

**Synchronous `GetConnection` in async method** — Warning — Code fix: yes

A call to the synchronous `GetConnection()` inside an `async` method of a `RepositoryBase`-derived class. The sync call blocks a thread-pool thread; use the async overload. The fix rewrites to `await GetConnectionAsync(...).ConfigureAwait(false)`.

Details: [LARC040](LARC040.md)

---

## LARC041

**Missing transaction in repository call** — Info — Code fix: yes

A Dapper or `DbConnection` operation call inside a `RepositoryBase`-derived class that does not pass the `Transaction` property. Without it the operation runs outside the ambient unit-of-work transaction. This is a presence check only — passing `transaction: Transaction` satisfies the rule even when `Transaction` is null at runtime (self-managed mode). The fix appends `transaction: Transaction` to the call.

Details: [LARC041](LARC041.md)

---

## LARC042

**Direct `DbConnection` instantiation in repository** — Warning — Code fix: no

A `new <DbConnection subtype>(...)` inside a `RepositoryBase`-derived class. Repositories must acquire connections via `GetConnection()`/`GetConnectionAsync()` so connection management stays centralized.

Details: [LARC042](LARC042.md)

---

## LARC043

**`ReleaseConnection` called with null argument** — Warning — Code fix: no

A `ReleaseConnection(null)` (or `default`) call on `RepositoryBase`. Releasing null is a no-op that usually means the acquired connection variable was lost or never assigned — pass the actual connection variable instead.

Details: [LARC043](LARC043.md)

---

## LARC044

**Missing `ReleaseConnection` after `GetConnection`** — Warning — Code fix: no

A `GetConnection()`/`GetConnectionAsync()` call with no matching `ReleaseConnection(...)` in a `finally` block of the same method. `ReleaseConnection` no-ops when the connection came from an externally-provided `DbConnection` or an active `DbTransaction`, so it is the one correct disposal path in both self-managed and unit-of-work modes — a bare `using` would close a connection the repository does not own.

Details: [LARC044](LARC044.md)

---

## LARC045

**Repository never uses `Transaction`** — Info — Code fix: no

A non-abstract `RepositoryBase`-derived class that makes at least one database-operation call but never references the `Transaction` property anywhere in the type. Such a repository cannot participate in unit-of-work mode. The per-call version of this check is LARC041.

Details: [LARC045](LARC045.md)
