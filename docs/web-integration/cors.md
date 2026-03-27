# CORS.AspNetCore Usage Guide

The **CORS.AspNetCore** library provides quick-start extension methods to configure Cross-Origin Resource Sharing (CORS) in ASP.NET Core applications.

## Setup

### 1. Register Policies
In your `Program.cs`, add the policies to the service collection:

```csharp
using LightningArc.CORS.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Adds predefined CORS policies (including "AllowAll")
builder.Services.AddCorsPolicies(); 
```

### 2. Enable Middleware
Enable the CORS middleware in the request pipeline:

```csharp
var app = builder.Build();

// Uses the "AllowAll" policy by default
app.UseCorsPolicies(); 

app.Run();
```

## Available Policies

| Policy Name | Description | Security |
| :--- | :--- | :--- |
| **"AllowAll"** | Allows any origin, any header, and any method (`*`). | **Low** (Use for dev only) |

## Manual Usage
If you registered the policies using `AddCorsPolicies()` but want to apply them only to specific endpoints:

```csharp
app.MapGet("/public-data", () => "Data").RequireCors("AllowAll");
```

---

## Implementation Details

For deep dives into the source code, see:
*   [CORS.AspNetCore Implementation](../advanced/internals/Web/CORS.AspNetCore/README.md)
*   [AllowAll Policy](../advanced/internals/Web/CORS.AspNetCore/CORS/Policies/AllowAll.md)
