# TaskTypeFactory.cs Implementation Details

**File Path:** `Meta/Metalama/Factories/Factories/TaskTypeFactory.cs`
**Namespace:** `LightningArc.Metalama`

## Overview
A static factory class executing at compile-time. It caches and generates `INamedType` references for `Task`, `Task<T>`, `ValueTask`, and `ValueTask<T>`.

## Code Analysis

### Caching
It caches the `INamedType` for base types (`_taskIType`, `_genericTaskIType`) to avoid repeated lookups during the compilation of large projects.

### `MakeGenericInstance`
The methods `GetTaskType(IType type)` use `_genericTaskIType.MakeGenericInstance(type)` to construct the specific closed generic type (e.g., `Task<int>`) needed for code generation.
