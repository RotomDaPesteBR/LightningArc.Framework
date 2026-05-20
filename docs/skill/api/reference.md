# Technical API Reference (Summary)

This is a self-contained reference for the most important types across the `LightningArc` ecosystem.

## `LightningArc.Results`
- **`Result` / `Result<T>`**: Outcome wrapper with boolean operators (`if (res)`) and deconstruction.
- **`Error` / `AggregateError`**: Hierarchical error codes. Supports `+` operator for aggregation.
- **`Success`**: Standardized success metadata (Ok, Created, etc.).

## `LightningArc.Results.AspNetCore`
- **`EndpointResult<T>`**: Implicitly converts `Result<T>` to `IResult` with Problem Details support.
- **`AddEndpointResults()`**: DI registration for mapping and global exception handling.

## `LightningArc.Data.ADO.SqlBuilder`
- **`SqlBuilder`**: Entry point for fluent SQL creation.
  - `.Select(table)`, `.Insert(table)`, `.Update(table)`, `.Delete(table)`
  - `.Where(column, value)`, `.Columns(...)`, `.OrderBy(col, direction)`

## `LightningArc.Data.Abstractions`
- **`IDbRepository<T, TKey>`**: Standardized data access interface.
- **`IUnitOfWork`**: Transaction management (`CommitAsync`, `RollbackAsync`).

## `LightningArc.Metalama`
- **`NamedTypeFactory`**: Static factory to resolve `Result` types at compile-time.
- **`ITypeExtensions`**: Extension methods like `.IsResult()`.
- **`ResultTypeFactory`**: Advanced factories for generating `Result` instances in templates.
