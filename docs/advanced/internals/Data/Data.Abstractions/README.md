# Data.Abstractions Implementation

This library defines the core interfaces and base classes for the Data Access Layer.

## Factories (`Factories/`)

*   **[IConnectionFactory](Factories/IConnectionFactory.md)**: Abstraction for creating DB connections.
*   **[IRepositoryFactory](Factories/IRepositoryFactory.md)**: Abstraction for creating repositories.
*   **[RepositoryFactory](Factories/RepositoryFactory.md)**: Concrete implementation for creating repositories with optional mapping and logging.

## Repositories (`Repositories/`)

*   **[IRepository](Repositories/IRepository.md)**: Contract for self-creating repositories (supports Static Abstracts).
*   **[RepositoryBase](Repositories/RepositoryBase.md)**: Unified base class managing connection lifecycle and optional mapping.

## Unit of Work (`UnitOfWork/`)

*   **[IUnitOfWork](UnitOfWork/IUnitOfWork.md)**: Transaction management interface.

## Mappers (`Mappers/`)

*   **[IMapper](Mappers/IMapper.md)**: Abstraction for mapping libraries.
