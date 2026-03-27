# RepositoryFactory.cs Implementation Details

**File Path:** `Data/Data.Abstractions/Factories/RepositoryFactory.cs`
**Namespace:** `LightningArc.Data.Abstractions.Factories`

## Overview
A concrete, sealed implementation of `IRepositoryFactory`. It acts as the central hub for creating both Pure and Mapped repositories.

## Code Analysis

### Constructor
```csharp
public RepositoryFactory(
    IConnectionFactory connectionFactory,
    IMapper? mapper = null,
    ILoggerFactory? loggerFactory = null
)
```
*   **Optional Mapper**: The `IMapper` is nullable. This allows the factory to be used in contexts where object mapping is not needed (e.g., pure ADO.NET projects).
*   **Optional Logging**: Uses `ILoggerFactory` to create specific loggers for each repository type created.

### Creating Pure Repositories
Methods like `Create<T>()` allow creating repositories that implement `IRepository<T>`. They ignore the internal `_mapper`.

### Creating Mapped Repositories
Methods like `CreateMapped<T>()` allow creating repositories that implement `IMappedRepository<T>`.
*   **Validation**: They call `EnsureMapper()`, which throws `InvalidOperationException` if the factory was initialized without a mapper.
*   **Injection**: They pass the `_mapper` instance to the repository's `Create` method.
