---
uid: result-pattern
level: 100
summary: "Comprehensive guide on the Functional Result Pattern implementation in LightningArc.Utils, detailing how to replace exceptions with robust success/failure models."
keywords: "Result Pattern, Functional Programming, Error Handling, Success, Failure, C#, LightningArc"
---

# Functional Result Pattern

The Result pattern is a functional programming concept that represents the outcome of an operation as a single object, which can either be a **Success** or a **Failure**. This approach avoids the use of exceptions for expected business logic failures, leading to more predictable and maintainable code.

## Core Concepts

In `LightningArc.Utils`, the pattern is implemented via two main classes:
- `Result`: Represents an operation with no return value.
- `Result<TValue>`: Represents an operation that returns a value of type `TValue` on success.

### Why use Result Pattern?

1. **Explicit Error Handling**: The compiler forces you to acknowledge that an operation can fail.
2. **Performance**: Avoids the high overhead of throwing and catching exceptions.
3. **Flow Control**: Enables a fluent, pipeline-based approach to business logic.

## Creating Results

### Success
```csharp
// Non-generic success (200 OK equivalent)
var res = Result.Success();

// Generic success with value
var valRes = Result.Success(42);

// Semantic success types
var created = Result.Created(newItem);
var accepted = Result.Accepted();
```

### Failure
```csharp
// Using predefined error modules
var error = Error.Resource.NotFound("User not found");
var fail = Result.Failure(error);

// Implicit conversion
Result<User> implicitFail = Error.Validation.InvalidFormat("Invalid email");
```

## Chaining Operations

The library provides powerful extension methods to chain operations:

| Method | Description |
|--------|-------------|
| `Map` | Transforms the success value. Skips on failure. |
| `Bind` | Chains another operation that returns a `Result`. |
| `Ensure` | Validates a condition. If false, turns success into failure. |
| `Tap` | Executes a side-effect on success without changing the value. |
| `Match` | Unwraps the result into a final value by handling both cases. |

### Example Chain
```csharp
public Result<UserProfile> UpdateUser(int id, UpdateDto dto)
{
    return _repo.FindById(id)
        .Ensure(user => !user.IsLocked, Error.Authentication.Forbidden("User locked"))
        .Map(user => UpdateProperties(user, dto))
        .Bind(user => _repo.Save(user))
        .Tap(user => _logger.LogInformation($"User {id} updated"));
}
```

## Async Support

Use `TaskResult<T>` for asynchronous flows. It is awaitable and supports fluent chaining.

```csharp
public TaskResult<User> GetUserAsync(int id) => _service.Fetch(id);

// Usage
var result = await GetUserAsync(1)
    .MapAsync(u => u.ToDto());
```
