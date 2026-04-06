# LightningArc.Core

**LightningArc.Core** is a comprehensive set of C# utilities and patterns designed to promote clean, expressive, and robust code in .NET applications. It focuses on Functional Error Handling, Domain-Driven Design primitives, and seamless ASP.NET Core integration.

## 📚 Documentation

Detailed documentation is available in the **[Docs](Docs/README.md)** directory.

*   **[Usage Guides](Docs/Usage/README.md)**: How to install and use the libraries.
*   **[Implementation Details](Docs/Implementation/README.md)**: Deep dive into the source code and internal logic.

## 📦 Modules

The solution is divided into three main layers:

### 1. Core Layer
The foundation of the ecosystem, compatible with any .NET application.

*   **`LightningArc.Core.Results`**: A robust implementation of the **Result Pattern** (Success/Failure) to replace exceptions for flow control.
*   **`LightningArc.Core.Abstractions`**: Common base types and Value Objects (e.g., `Email`) with built-in validation.
*   **`LightningArc.Core.Json`**: `System.Text.Json` converters to serialize Value Objects as simple types.

### 2. Web Layer
Extensions and adapters for ASP.NET Core applications.

*   **`LightningArc.Results.AspNetCore`**: Automatically translates domain `Result<T>` objects into HTTP Responses (`IResult`), supporting **RFC 7807 Problem Details**.
*   **`LightningArc.CORS.AspNetCore`**: Pre-configured CORS policies (like "AllowAll" for development).
*   **`LightningArc.OpenAPI.AspNetCore`**: Transformers to ensure your Value Objects are correctly documented in Swagger/OpenAPI.

### 3. Meta Layer
Metaprogramming tools powered by **Metalama**.

*   **`LightningArc.Core.Metalama`**: Extensions and Factories to simplify the creation of Aspects (AOP), such as generating async wrappers or injecting method parameters at compile time.

## 🚀 Quick Start

### Error Handling with Results

```csharp
using LightningArc.Core.Results;

public Result<User> CreateUser(string email)
{
    if (string.IsNullOrEmpty(email))
    {
        return Error.Validation("Email is required");
    }

    return new User(email); // Implicit conversion to Success
}
```

### ASP.NET Core Integration

```csharp
// Program.cs
builder.Services.AddEndpointResults();

// Endpoint
app.MapPost("/users", (UserDto dto) => 
{
    // Returns Result<User> -> converted to HTTP 200/400 automatically
    return userService.Create(dto); 
});
```

## 🛠 Building

The project uses a custom PowerShell script to build and pack all artifacts.

```powershell
.\publish.ps1
```

This will generate NuGet packages in `Publish/Packages/{Version}`.

## 📄 License

This project is proprietary/private. (Update this section if open source).

