# IUnitOfWork.cs Implementation Details

**File Path:** `Data/Data.Abstractions/UnitOfWork/IUnitOfWork.cs`
**Namespace:** `LightningArc.Data.Abstractions.UnitOfWork`

## Overview
Defines the **Unit of Work** pattern interface. This pattern is responsible for maintaining a list of objects affected by a business transaction and coordinating the writing out of changes and the resolution of concurrency problems.

## Code Analysis

### Transaction Management
*   **`Begin()` / `BeginAsync()`**: Starts a database transaction. All repositories sharing the `IUnitOfWork`'s connection will participate in this transaction.
*   **`Commit()` / `CommitAsync()`**: Persists changes.
*   **`Rollback()` / `RollbackAsync()`**: Undoes changes.

### Shared State
*   `Connection`: The shared `DbConnection`.
*   `Transaction`: The shared `DbTransaction`.

### Usage
Implementations of this interface typically act as a factory for Repositories, ensuring that when a repository is requested, it receives the `Connection` and `Transaction` managed by this Unit of Work.
