# EmailSchemaTransformer.cs Implementation Details

**File Path:** `Web/OpenAPI.AspNetCore/Filters/EmailSchemaTransformer.cs`
**Namespace:** `LightningArc.OpenAPI.AspNetCore.Filters`

## Overview
This class implements `IOpenApiSchemaTransformer`, a new interface in ASP.NET Core (NET 9+) for customizing OpenAPI (Swagger) schema generation. Its purpose is to ensure that the `Email` Value Object is documented as a simple `string` rather than a complex object.

## Code Analysis

### `TransformAsync`
```csharp
public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
```
*   **Target Check**: It checks `context.JsonTypeInfo?.Type` to see if the current type being processed is `typeof(Email)`.
*   **Transformation**:
    *   **Properties**: Clears existing properties (removes `Value` property from the documentation).
    *   **Type**: Sets the schema type to `"string"` (or `JsonSchemaType.String` in NET 10).
    *   **Format**: Sets the format to `"email"`, which helps UI tools like Swagger UI to validate input.
    *   **Example**: Adds a default example (`"usuario@exemplo.com"`).
*   **Conditionals**: Uses `#if NET9_0` and `#if NET10_0` preprocessor directives to handle breaking changes or API evolution in the underlying library.

### Impact
Without this transformer, Swagger UI would show `Email` as:
```json
{
  "value": "string"
}
```
With this transformer, it shows:
```json
"usuario@exemplo.com"
```
This matches the actual serialization behavior defined in `Json`.
