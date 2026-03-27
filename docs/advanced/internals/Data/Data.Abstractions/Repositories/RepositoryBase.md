# RepositoryBase.cs Implementation Details

**File Path:** `Data/Data.Abstractions/Repositories/RepositoryBase.cs`
**Namespace:** `LightningArc.Data.Abstractions.Repositories`

## Overview
The foundation for all repositories. It provides the plumbing for managing database connections, transactions, and optional object mapping.

## Code Analysis

### Connection Management
*   **`GetConnection()`**: Intelligent retrieval. It checks if there is an active `_dbConnection`. If not, it uses the `_connectionFactory` to create a new one.
*   **`FinalizeConnection()`**: Clean-up logic. It **only** closes the connection if it was created locally.

### Optional Mapping
*   **`Mapper` field**: A `protected readonly IMapper? Mapper` field is available to all inheriting classes.
*   **Plug-and-Play**: If an `IMapper` is provided to the constructor (usually via `RepositoryFactory`), it can be used for model-entity conversion. If not, it remains `null`.

### Generics (`RepositoryBase<TModel, TEntity>`)
A specialized version for repositories that handle specific types. It provides type safety and serves as a target for AOP (Aspect-Oriented Programming) code generation.