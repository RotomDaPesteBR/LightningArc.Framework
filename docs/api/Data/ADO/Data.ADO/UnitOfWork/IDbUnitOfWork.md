# IDbUnitOfWork

**Namespace:** `LightningArc.Data.ADO.UnitOfWork`  
**Type:** `interface`

A specialized version of `IUnitOfWork` for database-level operations (Dapper/ADO.NET). It exposes the underlying `DbConnection` and `DbTransaction` for granular control.

---

## Properties

*   **Connection** (`DbConnection`): Gets the active database connection.
*   **Transaction** (`DbTransaction?`): Gets the active database transaction (may be null if not started).

---

## Inherited Methods
See [IUnitOfWork](../../../Data.Abstractions/UnitOfWork/IUnitOfWork.md) for inherited transaction control methods.
