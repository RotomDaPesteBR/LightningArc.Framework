---
uid: data-access
level: 100
summary: "Conceptual guide on Data Access patterns, Repositories, Unit of Work, and Mapper adapters."
keywords: "Repository, Unit of Work, ADO.NET, Dapper, Entity Framework, AutoMapper, Mapster"
---

# Data Access & Mappers

The library provides abstractions and concrete implementations for common data access patterns, supporting both high-level ORMs like Entity Framework and low-level tools like Dapper/ADO.NET.

## Repository Pattern

Repositories are abstracted via `IRepository<TEntity>`. Specialized versions exist for different technologies:

### ADO.NET / Dapper
Use `IDbRepository<T>` for repositories that need direct access to `DbConnection` and `DbTransaction`.
- **Static Creation**: Supports `.Create()` static abstract methods for factory-based instantiation.

### Entity Framework Core
Use `RepositoryBase<TEntity, TContext>` to implement standard EF repositories with built-in `DbSet` management.

## Unit of Work (UoW)

Coordinating multiple repository operations within a single transaction is handled by `IUnitOfWork`.

- **Begin/Commit/Rollback**: Standard transactional methods.
- **IDbUnitOfWork**: Specialized for ADO.NET, exposing the underlying `Connection` and `Transaction`.

## Object Mapping (IMapper)

The library abstracts object-to-object mapping, allowing you to swap mapping libraries easily.

### Available Adapters
- `Mappers.AutoMapper`: Adapter for the AutoMapper library.
- `Mappers.Mapster`: Adapter for the Mapster library.

### Registration
```csharp
// In Dependency Injection
services.AddAutoMapperAdapter(cfg => {
    cfg.CreateMap<User, UserDto>();
});

// Or Mapster
services.AddMapsterAdapter();
```

### Usage
```csharp
public class MyService(IMapper mapper) {
    public UserDto GetDto(User user) => mapper.Map<UserDto>(user);
}
```
