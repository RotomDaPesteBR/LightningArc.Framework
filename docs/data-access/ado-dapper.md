# Data.ADO Usage Guide

**Data.ADO** provides the base implementation for repositories using ADO.NET or micro-ORMs like Dapper. It includes specific implementations for Oracle and SQL Server.

## Implementation Example (Dapper + Result Pattern)

This example shows how to implement a repository method using Dapper, ensuring proper connection lifecycle management and using the `Result` pattern for error handling.

```csharp
public class UserRepository : RepositoryBase<UserEntity, UserDto>
{
    public UserRepository(IConnectionFactory factory, IMapper? mapper = null) 
        : base(factory, mapper) { }

    public async Task<Result<IEnumerable<UserDto>>> GetAllAsync(CancellationToken ct = default)
    {
        const string query = "SELECT * FROM Users";
        DbConnection? connection = null;

        try
        {
            connection = await GetConnectionAsync(ct);

            var entities = await connection.QueryAsync<UserEntity>(
                new CommandDefinition(
                    query,
                    transaction: _transaction, // Uses the shared transaction if present
                    cancellationToken: ct
                )
            );

            // Mapping to DTO
            var dtos = Mapper != null 
                ? Mapper.Map<IEnumerable<UserDto>>(entities) 
                : entities.Cast<UserDto>();

            return Result.Success(dtos);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to fetch users");
            return Result.Failure<IEnumerable<UserDto>>("Database error occurred");
        }
        finally
        {
            FinalizeConnection(connection);
        }
    }
}
```

### Base Repositories

*   **`RepositoryBase`**: The non-generic base class.
*   **`RepositoryBase<TEntity>`**: For repositories where the entity is returned directly.
*   **`RepositoryBase<TEntity, TResult>`**: Supporting mapping (e.g., from DB entity to DTO).

```csharp
public class MyRepository : RepositoryBase<MyEntity, MyDto>
{
    public MyRepository(IConnectionFactory factory, IMapper? mapper = null) 
        : base(factory, mapper) { }

    public async Task<MyDto> GetAsync(int id)
    {
        DbConnection? connection = null;
        try 
        {
            connection = GetConnection();
            // Use connection with Dapper or ADO.NET
            // var result = await connection.QuerySingleAsync<MyEntity>(...);
            // return Mapper.Map<MyDto>(result);
        }
        finally 
        {
            FinalizeConnection(connection);
        }
    }
}


## Connection Factories

Connection factories are responsible for creating `DbConnection` instances. We provide specialized implementations for different database providers.

### SQL Server (`Data.ADO.SqlServer`)

Uses `Microsoft.Data.SqlClient`.

```csharp
// Using IConfiguration (reads "ConnectionStrings:DatabaseConnection")
var factory = new SqlConnectionFactory(configuration);

// Or using a direct connection string
var factory = new SqlConnectionFactory("Server=...;Database=...;");
```

### Oracle (`Data.ADO.Oracle`)

Uses `Oracle.ManagedDataAccess.Client`.

```csharp
// Using IConfiguration (reads "ConnectionStrings:DatabaseConnection")
var factory = new OracleConnectionFactory(configuration);

// Or using a direct connection string
var factory = new OracleConnectionFactory("User Id=...;Password=...;Data Source=...");
```

## Dependency Injection

Register the appropriate factory in your `Program.cs` or `Startup.cs`:

```csharp
// For SQL Server
services.AddSingleton<IConnectionFactory, SqlConnectionFactory>();

// For Oracle
services.AddSingleton<IConnectionFactory, OracleConnectionFactory>();
```
