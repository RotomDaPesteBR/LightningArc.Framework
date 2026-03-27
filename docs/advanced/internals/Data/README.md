# Data Layer Implementation

The Data layer provides patterns and tools for database access.

## Projects

*   **[Data.Abstractions](Data.Abstractions/README.md)**: Core interfaces (Unit of Work, Repository, Mapper) and base classes.
*   **ADO/**: Projects related to ADO.NET and Dapper.
    *   `Data.ADO`: Base implementations.
    *   `Data.ADO.Oracle`: Oracle driver integration.
    *   `Data.ADO.SqlServer`: SQL Server driver integration.
*   **EF/**: Projects related to Entity Framework Core.
    *   `Data.EntityFramework`: Generic base for EF.
*   **Mappers/**: Object mapping adapters.
    *   `Mappers.AutoMapper`: Adapter for AutoMapper library.
    *   `Mappers.Mapster`: Adapter for Mapster library.
