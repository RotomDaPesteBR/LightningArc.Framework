# IRepository.cs Implementation Details

**File Path:** `Data/Data.Abstractions/Repositories/IRepository.cs`
**Namespace:** `LightningArc.Data.Abstractions.Repositories`

## Overview
Defines the contract for a "Self-Creating" repository.

## Code Analysis

### Static Abstracts (.NET 7+)
The interface uses **Static Abstract Members in Interfaces**.
```csharp
public static abstract TRepository Create(...);
```
This forces implementing classes to define static factory methods.

### Unified Signature
The contract includes an optional `IMapper? mapper` parameter. This allows repositories to decide internally if they need mapping capabilities without changing the interface or the factory logic.

### Backward Compatibility
For `netstandard2.0`, these members are hidden via `#if NET7_0_OR_GREATER`. In this case, the `RepositoryFactory` uses Reflection to find and invoke the `Create` method by convention.