# IAspectBuilderExtensions.cs Implementation Details

**File Path:** `Meta/Metalama/Extensions/Extensions/IAspectBuilderExtensions.cs`
**Namespace:** `LightningArc.Metalama`

## Overview
This class provides `[CompileTime]` extension methods for `IAspectBuilder<INamedType>`. Its primary purpose is to simplify the introduction of new methods (both synchronous and asynchronous) into types during the compilation process (Metaprogramming).

## Code Analysis

### `AddMethod`
```csharp
public static IAspectBuilder<INamedType> AddMethod(...)
```
*   **Role**: Wrapper around `builder.IntroduceMethod`.
*   **Simplification**: It abstracts away the complexity of configuring the `IntroduceMethod` call, providing a cleaner API for defining the method name, return type, and parameters.
*   **Parameters**:
    *   `templateName`: The name of the T# template method to be used as the body.
    *   `methodName`: The name of the new method to be generated.
    *   `args`: Arguments to be passed to the template.

### `AddAsyncMethod`
```csharp
public static IAspectBuilder<INamedType> AddAsyncMethod(..., AsyncType asyncType = AsyncType.Task)
```
*   **Role**: Specialized version of `AddMethod` for async methods.
*   **Automatic Naming**: Appends "Async" to the method name.
*   **Return Type Wrapping**: Automatically wraps the return type in `Task<T>` or `ValueTask<T>` using `TaskTypeFactory`.
*   **Benefit**: Reduces boilerplate in aspects that need to generate async wrappers or proxies.
