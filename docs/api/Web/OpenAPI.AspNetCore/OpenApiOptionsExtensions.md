# OpenApiOptionsExtensions

**Namespace:** `LightningArc.OpenAPI.AspNetCore`  
**Type:** `static partial class`

Provides extension methods for configuring OpenAPI (Swagger) options in ASP.NET Core, specifically for handling custom types like Value Objects.

---

## Extension Methods

### AddSchemaTransformers(this OpenApiOptions openApiOptions)
*   **Description:** Adds schema transformers to handle custom value objects in the OpenAPI documentation. For example, it registers the `EmailSchemaTransformer` so that `Email` types appear correctly as strings in the Swagger UI.
*   **Returns:** `OpenApiOptions` for chaining.

---

## Usage Example

```csharp
// Program.cs
builder.Services.AddOpenApi(options => {
    options.AddSchemaTransformers();
});
```
