# OpenApiOptionsExtensions.cs Implementation Details

**File Path:** `Web/OpenAPI.AspNetCore/OpenApiOptionsExtensions.cs`
**Namespace:** `LightningArc.OpenAPI.AspNetCore`

## Overview
Provides an extension method to register all schema transformers provided by the library.

## Code Analysis

### `AddSchemaTransformers`
```csharp
public static OpenApiOptions AddSchemaTransformers(this OpenApiOptions openApiOptions)
```
*   **Usage**: Extends `OpenApiOptions` (typically accessed within `MapOpenApi` or `AddOpenApi`).
*   **Logic**: Registers `EmailSchemaTransformer`.
*   **Purpose**: Simplifies configuration for the end user.
