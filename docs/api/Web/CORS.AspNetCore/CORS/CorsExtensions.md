# CorsExtensions

**Namespace:** `LightningArc.CORS.AspNetCore`  
**Type:** `static class`

Provides extension methods to configure predefined CORS (Cross-Origin Resource Sharing) policies in ASP.NET Core applications.

---

## Extension Methods

### AddCorsPolicies(this IServiceCollection services)
*   **Description:** Registers predefined CORS policies in the service collection. Currently, it adds the "AllowAll" policy.
*   **Returns:** `IServiceCollection` for chaining.

### UseCorsPolicies(this IApplicationBuilder app)
*   **Description:** Configures the application pipeline to use the predefined CORS policies (e.g., "AllowAll").
*   **Returns:** `IApplicationBuilder` for chaining.

---

## Usage Example

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddCorsPolicies();

var app = builder.Build();

// Use middleware
app.UseCorsPolicies();

app.Run();
```

