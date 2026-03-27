# AllowAll.cs Implementation Details

**File Path:** `Web/CORS.AspNetCore/CORS/Policies/AllowAll.cs`
**Namespace:** `LightningArc.CORS.AspNetCore.Policies`

## Overview
The `AllowAll` class provides a pre-configured CORS policy that relaxes all cross-origin restrictions.

## Code Analysis

### `AddAllowAllPolicy`
```csharp
public static IServiceCollection AddAllowAllPolicy(this IServiceCollection services)
```
*   **Purpose**: Registers a CORS policy named `"AllowAll"` in the DI container.
*   **Logic**:
    *   Creates a new `CorsPolicy` instance.
    *   Adds `*` to `Headers`, `Methods`, and `Origins`.
    *   Uses `services.AddCors()` to register the policy with the name `"AllowAll"`.
*   **Security Warning**: This policy is highly permissive and should be used with caution, primarily in development environments or very specific public API scenarios.

