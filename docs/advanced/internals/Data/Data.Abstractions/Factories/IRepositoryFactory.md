# IRepositoryFactory.cs Implementation Details

**File Path:** `Data/Data.Abstractions/Factories/IRepositoryFactory.cs`
**Namespace:** `LightningArc.Data.Abstractions.Factories`

## Overview
Defines a factory capable of creating instances of specific repositories. It distinguishes between "Standard" (Pure) repositories and "Mapped" repositories.

## Code Analysis

### Standard Repositories
```csharp
T Create<T>() where T : IRepository<T>;
```
Creates repositories that do not require an object mapper.

### Mapped Repositories
```csharp
T CreateMapped<T>() where T : IMappedRepository<T>;
```
Creates repositories that typically map between Data Models and Domain Entities. Implementations must ensure a Mapper is available.

### Overloads
Both categories support creating repositories:
1.  **Default**: Using the factory's internal configuration.
2.  **Explicit ConnectionFactory**: Overriding the connection source.
3.  **Explicit Connection/Transaction**: For Unit of Work participation.