# LightningArc.Framework

**LightningArc** is a modular, production-ready .NET framework for building consistent applications by combining structured error handling, DDD primitives, and seamless ASP.NET Core integration.

It enforces best practices at compile time through Roslyn analyzers, helping developers write safer and more predictable code without relying on runtime conventions.

Built from real-world usage, LightningArc is actively used in production APIs to standardize architecture, improve reliability, and reduce boilerplate.

---

## Why LightningArc?

LightningArc is not just a collection of utilities — it is a cohesive framework designed to standardize how applications handle errors, validation, and API responses.

It provides:

* Compile-time enforcement with Roslyn analyzers
* Structured domain errors with codes, messages, and contextual details
* Built-in Result pattern and strongly typed Value Objects
* First-class ASP.NET Core integration (RFC 7807 Problem Details)
* A unified model across domain, data, and web layers

Designed for real-world systems, LightningArc helps teams reduce boilerplate, enforce consistency, and avoid common pitfalls.

---

## Getting Started

Install the meta package:

```bash
dotnet add package LightningArc.Framework
```

For ASP.NET Core integration:

```bash
dotnet add package LightningArc.AspNetCore
```

Or install individual modules directly:

```bash
dotnet add package LightningArc.Results
dotnet add package LightningArc.Results.AspNetCore
dotnet add package LightningArc.Primitives
```

---

## Package Overview

* `LightningArc.Framework` → Meta package for the core layer
* `LightningArc.AspNetCore` includes common ASP.NET Core integrations, while specialized packages like `LightningArc.Results.AspNetCore` provide focused functionality.
* `LightningArc.Results` → Core result and error handling
* `LightningArc.Results.AspNetCore` → Result-to-HTTP mapping for ASP.NET Core
* `LightningArc.Primitives` → Value Objects

This is a simplified overview. See the Modules section below for full details.

---

## Modules

The solution is organized into four layers:

### Core Layer

Foundation types and patterns, framework-agnostic.

| Project                               | Description                                                        |
| ------------------------------------- | ------------------------------------------------------------------ |
| **`LightningArc.Results`**            | Complete Result pattern (Success/Error) with chaining operators. |
| **`LightningArc.Primitives`**         | Value Objects: `Email`, `Cpf`, `Cnpj`, `PhoneNumber`, `Url`.       |
| **`LightningArc.Primitives.Results`** | Result-based extensions for ValueObjects.                          |
| **`LightningArc.Json`**               | `System.Text.Json` converters and helpers.                         |
| **`LightningArc.Framework`**          | Meta package with all core layer packages.                         |

### Data Layer

Repository patterns and ADO.NET helpers.

| Project                                 | Description                                                    |
| --------------------------------------- | -------------------------------------------------------------- |
| **`LightningArc.Data.Abstractions`**    | `IRepository`, `IUnitOfWork`, `IConnectionFactory` interfaces. |
| **`LightningArc.Data.ADO`**             | Base `RepositoryBase` with `GetConnectionAsync` support.       |
| **`LightningArc.Data.ADO.SqlServer`**   | `SqlConnectionFactory` for SQL Server.                         |
| **`LightningArc.Data.ADO.Oracle`**      | `OracleConnectionFactory` for Oracle.                          |
| **`LightningArc.Data.ADO.SqlBuilder`**  | Metadata-driven SQL statement builder.                         |
| **`LightningArc.Data.EntityFramework`** | EF Core repository base.                                       |
| **`LightningArc.Mappers.AutoMapper`**   | AutoMapper adapter for data layer.                             |
| **`LightningArc.Mappers.Mapster`**      | Mapster adapter for data layer.                                |

### Web Layer

ASP.NET Core integrations.

| Project                               | Description                                                   |
| ------------------------------------- | ------------------------------------------------------------- |
| **`LightningArc.AspNetCore`**         | Core ASP.NET Core utilities.                                  |
| **`LightningArc.Results.AspNetCore`** | Map `Result<T>` to HTTP responses (RFC 7807 Problem Details). |
| **`LightningArc.CORS.AspNetCore`**    | Pre-configured CORS policies.                                 |
| **`LightningArc.OpenAPI.AspNetCore`** | OpenAPI/Swagger transformers for ValueObjects.                |

### Meta & Analyzers

Compile-time tools.

| Project                             | Description                                       |
| ----------------------------------- | ------------------------------------------------- |
| **`LightningArc.Analyzers`**        | 9 Roslyn analyzer rules + 2 automatic code fixes. |
| **`LightningArc.Metalama`**         | Metalama-powered AOP factories.                   |
| **`LightningArc.Metalama.Results`** | Result-related Metalama utilities.                |

---

## Documentation

Detailed documentation is available in the **[Docs](docs/README.md)** directory:

* **[Getting Started](docs/getting-started/introduction.md)**
* **[Core Features](docs/core-features/result-pattern.md)**
* **[Data Access](docs/data-access/abstractions.md)**
* **[Web Integration](docs/web-integration/results-mapping.md)**
* **[Analyzers](docs/analyzers/README.md)**
* **[AI Skill](docs/skill/SKILL.md)**

---

## Quick Start

### Error Handling with Results

```csharp
using LightningArc.Results;

public Result<User> CreateUser(string email, string password)
{
    if (string.IsNullOrEmpty(email))
        return Error.Validation.MissingField("Email is required");

    if (password.Length < 8)
        return Error.Validation.ValueOutOfRange("Password must be at least 8 characters");

    // Implicitly converts User to Result<User> (Success Ok)
    return new User(email, password);
}
```

---

### Safe Value Object Creation

```csharp
using LightningArc.Primitives.ValueObjects;
using LightningArc.Primitives.Results;

// 1. Extension method (Recommended for strings)
Result<Email> result = input.AsEmail();

// 2. TryCreate (Standard .NET pattern)
if (Email.TryCreate(input, out var email)) { /* use it */ }

// 3. Currency with precision validation (ISO 4217)
Result<Currency> jpy = 100.50m.AsCurrency("JPY"); // Failure: JPY has 0 decimal places
Result<Currency> brl = 100.50m.AsCurrency("BRL"); // Success
```

### ASP.NET Core Integration

```csharp
// Program.cs
builder.Services.AddEndpointResults();

// Minimal API — maps Result<T> to HTTP responses (200, 201, 4xx, etc.)

// Explicit conversion (recommended)
app.MapPost("/users", (UserRequest request) =>
    userService.Create(request).ToEndpointResult());

// Implicit conversion (requires explicit return type)
app.MapPost("/users", (UserRequest request): EndpointResult<User> =>
    userService.Create(request));

// MVC Controller
[HttpGet("products/{id}")]
public EndpointResult<Product> GetProduct(string id)
{
    if (string.IsNullOrEmpty(id))
    {
        return Error.Validation.InvalidParameter("Product ID is required");
    }

    // Implicitly converts Result<Product> to EndpointResult<Product>
    return _productService.GetById(id);
}
```

```csharp
class UserService {
    public Result<User> Create(UserRequest request)
        => Result.Created(userRepository.Create(request)); // Returns a specific success result (201 Created)
}
```

---

## Advanced Usage

LightningArc provides advanced customization features for real-world applications, giving you full control over error modeling, response formats, and HTTP behavior.

You can centralize how your API responds to success and failures globally:

```csharp
// Program.cs
builder.Services.AddEndpointResults(
    wrapSuccessResponses: true, // Wraps success in a standard object
    // Sets the language for automatic error titles. 
    // If not provided, it defaults to the Current Thread Culture.
    defaultCulture: "en-US",
    configureMappings: (successes, errors) =>
    {
        // Custom mapping for business-specific errors
        errors.Map<Business.OrderRejectedError>(
            HttpStatusCode.UnprocessableEntity, 
            "Order Rejected", 
            "urn:api-errors:order-rejected"
        );
    },
    // Optional: Completely customize the success response format
    successResponseBuilder: (detail, httpContext) => new {
        timestamp = DateTime.UtcNow,
        path = detail.Instance,
        result = detail.Data
    }
);
```

### Custom Error Modules
Keep your business logic clean by creating specialized error modules:

```csharp
public class Business : Error.ErrorModule {
    public new const int CodePrefix = 12;

    public class OrderRejectedError : Error {
        internal OrderRejectedError(string message) 
            : base(Business.CodePrefix, 01, message) { }
    }
}

public static class BusinessErrorExtensions {
    public static Error OrderRejected(this Error.ErrorModule<Business> _, string message)
        => new Business.OrderRejectedError(message);
}

// Usage in Service/Controller
return Error.Custom<Business>().OrderRejected("Payment declined");
```

### Standardized Responses

**Wrapped Success (with `wrapSuccessResponses: true`)**
```json
{
  "status": 200,
  "message": "Operation completed successfully.",
  "instance": "/products/123",
  "data": {
    "id": "123",
    "name": "Mechanical Keyboard",
    "price": 99.90
  }
}
```

Note: Fields like `title` and `type` are customizable and can be configured globally or per error type using the mapping APIs.

**Error Response (RFC 7807 Problem Details)**
```json
{
  "type": "urn:api-errors:invalid-parameter",
  "title": "Validation_InvalidParameter",
  "status": 400,
  "detail": "One or more parameters in the input are invalid.",
  "instance": "/products/abc",
  "errors": [
    {
      "context": "id",
      "message": "The id field must be a valid GUID."
    }
  ]
}
```

### Using the Analyzers (Roslyn)

LightningArc packages automatically include the relevant Roslyn analyzers as transitive dependencies. Consumers do not normally need to install `LightningArc.Analyzers` separately.

The analyzer package can also be installed explicitly when consuming it independently:

```bash
dotnet add package LightningArc.Analyzers
```

The package provides rules covering Result safety, ValueObject usage, ADO.NET patterns, and Minimal API mapping, plus automatic code fixes. They ensure, for example, that you don't access `.Value` of a `Result` without checking `.IsSuccess` first, or that you don't use dangerous implicit conversions from strings to Value Objects in critical paths.


---

## Key Characteristics

* **Fail-Fast and Functional**: Prefer `Result` over exceptions for normal business flow.
* **Compile-Time Enforcement**: Roslyn analyzers catch misuse before runtime.
* **Extensible**: Most classes are designed for inheritance and extension.
* **Lightweight**: Minimal dependencies, optimized for high-throughput APIs.

---

## Building

The project uses centralized package management and MSBuild props. Build with:

```bash
dotnet build
```
