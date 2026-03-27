# Data Access & SQL Builder

The **`LightningArc.Data`** module provides abstractions for repositories and a metadata-driven SQL Builder.

## SQL Builder
**Namespace**: `LightningArc.Data.ADO.SqlBuilder`

The SQL Builder uses table metadata (column names, keys, ordering) to generate dialect-specific SQL statements.

### 1. Define Table Metadata
First, create a collection of `ColumnDefinition` describing your table structure.

```csharp
var columns = new List<ColumnDefinition>
{
    new("Id", isKey: true),
    new("Name"),
    new("Email"),
    new("CreatedAt", order: new OrderDefinition(OrderDirection.Descending, priority: 1))
};
```

### 2. Instantiate and Build
Instantiate the `SqlBuilder` with the table name, columns, and target dialect.

```csharp
var builder = new SqlBuilder("Users", columns, SqlDialect.SqlServer);

// Generate SELECT
string selectAll = builder.Select.Build();
string selectById = builder.Select.Build(filterByKey: true);

// Generate INSERT
string insert = builder.Insert.Build();

// Generate UPDATE
string update = builder.Update.Build();

// Generate DELETE
string delete = builder.Delete.Build();
```

---

## Repository & Unit of Work
**Namespace**: `LightningArc.Data.Abstractions`

### Repository Implementation
Always inherit from `RepositoryBase<T, TKey>` when using ADO.NET or Entity Framework to leverage standardized CRUD operations.

```csharp
public class UserRepo(IConnectionFactory factory) : RepositoryBase<User, int>(factory)
{
    // Inherits: FindAsync, AddAsync, UpdateAsync, DeleteAsync, etc.
}
```

### Unit of Work Pattern
Use `IUnitOfWork` to wrap multiple repository operations in a single transaction.

```csharp
using var uow = await _uowFactory.CreateAsync();
// ... execute repo calls ...
await uow.CommitAsync();
```
