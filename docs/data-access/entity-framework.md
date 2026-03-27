# Data.EntityFramework Usage Guide

**Data.EntityFramework** provides a generic base for repositories using Entity Framework Core, allowing you to use your own `DbContext` while benefiting from a standardized repository structure.

## Basic Usage

To implement a repository, inherit from `RepositoryBase<TContext, TEntity>`. You must specify your `DbContext` type and the entity type.

```csharp
// Your Application's DbContext
public class MyDbContext : DbContext 
{
    public DbSet<User> Users { get; set; }
}

// Your Repository implementation
public class UserRepository : RepositoryBase<MyDbContext, User>
{
    public UserRepository(MyDbContext context, ILogger<UserRepository> logger) 
        : base(context, logger: logger)
    {
    }

    public async Task<User?> GetActiveUser(int id)
    {
        // Use the protected DbSet or Context properties
        return await DbSet.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
    }
}
```

## Features

### Context and DbSet Access
The base class provides protected properties for easy access:
- `Context`: The typed `DbContext` instance.
- `DbSet`: The `DbSet<TEntity>` for the specific entity.
- `Logger`: An optional `ILogger` instance.
- `Mapper`: An optional `IMapper` instance (from `Data.Abstractions`).

### Dependency Injection
Since the base class uses generics for the context, the standard .NET DI container will automatically resolve your custom `DbContext` when injecting the repository:

```csharp
services.AddDbContext<MyDbContext>(options => ...);
services.AddScoped<UserRepository>();
```

## Generic Base Classes

- **`RepositoryBase<TContext>`**: Use this if you need a repository that works with multiple entities using the same context.
- **`RepositoryBase<TContext, TEntity>`**: The standard choice for single-entity repositories.
- **`RepositoryBase<TContext, TEntity, TResult>`**: Use this when you want to map the database entity to a different result model (e.g., a DTO).
