---
name: lightning-arc
description: Expert skill for the LightningArc C# library ecosystem. Specializing in functional error handling (Result Pattern), Domain-Driven Design (Value Objects), and clean architecture integration.
---

# LightningArc AI Skill

This skill provides the architectural constraints, coding standards, and API reference needed to build robust applications using the LightningArc ecosystem.

## 🛠️ Core Principles for AI

1.  **Always use `Result<T>`**: Never throw exceptions for expected business failures. Return `Error` types from the appropriate module.
2.  **Explicit over Implicit**: Prefer `Match` or `Bind` over manual `IsSuccess` checks where possible to maintain functional purity.
3.  **Modern C# Ergnonomics**: Leverage the library's syntactic sugar:
    *   `if (result)` instead of `if (result.IsSuccess)`.
    *   Deconstruction: `var (success, value, error) = result;`.
    *   Error Aggregation: `error1 + error2`.
4.  **Async by Default**: Use `TaskResult<T>` for all I/O operations.

## 📂 Knowledge Base Structure

| Section | Description |
|---------|-------------|
| `docs/getting-started/` | Installation and core philosophy. |
| `docs/core-features/` | Detailed guides on Result, Errors, and Value Objects. |
| `docs/web-integration/` | ASP.NET Core integration details. |
| `docs/api/` | Technical signatures for all public members. |
| `docs/skill/code/` | Reference implementations. |

## 🧩 Common Snippets & Patterns

### 1. Creating a Service Method
```csharp
public async TaskResult<UserDto> RegisterUser(string email, string password)
{
    // 1. Value Object Creation
    return Email.Create(email) 
        // 2. Functional Chaining
        .Bind(e => _authService.CheckUniqueness(e))
        .BindAsync(e => _repository.AddAsync(new User(e, password)))
        // 3. Mapping to DTO
        .Map(user => user.ToDto());
}
```

### 2. Handling Multiple Validations
```csharp
public Result ValidateOrder(OrderRequest request)
{
    Error? errors = null;
    
    if (string.IsNullOrEmpty(request.Id)) 
        errors += Error.Validation.MissingField("Id is required");
        
    if (request.Amount <= 0)
        errors += Error.Validation.ValueOutOfRange("Amount must be positive");
        
    return errors != null ? errors : Result.Success();
}
```

### 3. Controller Action (Cleanest way)
```csharp
[HttpPost]
public async Task<EndpointResult<UserDto>> Create(UserRequest req) => 
    await _service.RegisterUser(req.Email, req.Password);
```

## ⚠️ Coding Standards

- **Namespaces**: Always use `LightningArc.*`. Pruned namespaces are: `Results`, `Abstractions`, `Json`, `Data`, `Mappers`, `Metalama`.
- **Error Codes**: Composite code = `(Prefix * 1000) + Suffix`.
- **Aggregates**: When combining errors of different modules, the code becomes `99001` (General Aggregate).

## 🔍 API Discovery
If unsure about a method, look into:
- `docs/api/Core/Results/Results/Extensions/Result/` for fluent methods.
- `docs/api/Core/Results/Results/Errors/Modules/` for error types.
