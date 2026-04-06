# Data.Abstractions Usage Guide

**Data.Abstractions** provides foundational interfaces for implementing the Repository and Unit of Work patterns in a database-agnostic way.

## Concepts

### Repositories

Repositories abstract data access. This library uses a **Unified Repository Pattern**:

*   **`RepositoryBase`**: The base class for ADO.NET repositories (provided by **[Data.ADO](ado-dapper.md)**).
*   **Optional Mapping**: Support for object mapping (e.g., converting DB Models to Domain Entities) is built-in but optional. 
*   **Mapper as Plugin**: If you inject an `IMapper` implementation into the `RepositoryFactory`, it is automatically available in all created repositories.

### Unit of Work

The `IUnitOfWork` interface manages transactions, ensuring multiple repositories share the same `DbConnection` and `DbTransaction`.

## Implementing a Repository

```csharp
public class UserRepository : RepositoryBase<UserModel, User>, IRepository<UserRepository>
{
    public UserRepository(IConnectionFactory factory, IMapper? mapper) : base(factory, mapper) { }

    // Implement Static Factory (Required for RepositoryFactory)
    public static UserRepository Create(IConnectionFactory factory, IMapper? mapper, ILogger<UserRepository>? logger)
        => new UserRepository(factory, mapper);

    public async Task<User> GetByIdAsync(int id, CancellationToken ct = default)
    {
        DbConnection? connection = null;
        try
        {
            connection = await GetConnectionAsync(ct);
            // ... get UserModel from DB ...
            // return Mapper != null ? Mapper.Map<User>(userModel) : (User)userModel;
        }
        finally
        {
            ReleaseConnection(connection);
        }
    }
}
```

## Using the Factory

```csharp
// 1. Setup (in DI Container)
// Register IConnectionFactory
// Register IMapper (Optional - e.g., an adapter for Mapster)
// Register IRepositoryFactory -> RepositoryFactory

// 2. Usage
public class UserService(IRepositoryFactory factory)
{
    public void DoWork()
    {
        using var repo = factory.Create<UserRepository>();
        // ...
    }
}
```

---

## Implementation Details

For deep dives into the source code, see:
*   [Data.Abstractions Implementation](../advanced/internals/Data/Data.Abstractions/README.md)
*   [IRepository](../advanced/internals/Data/Data.Abstractions/Repositories/IRepository.md)
*   [IUnitOfWork](../advanced/internals/Data/Data.Abstractions/UnitOfWork/IUnitOfWork.md)
*   **Data.ADO** (for `RepositoryBase` and `IConnectionFactory`)