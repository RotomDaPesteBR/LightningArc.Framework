# Functional Result Pattern

The Result pattern represents the outcome of an operation as a single object, which can either be a **Success** or a **Failure**. This approach avoids exceptions for expected business logic failures.

## Core Types
- `Result`: For operations without a return value.
- `Result<TValue>`: For operations returning `TValue`.
- `Error`: Typed failure information with hierarchical codes.
- `AggregateError`: A collection of multiple errors (e.g., validation summary).

## Modern C# Ergonomics

### Boolean Logic
Use results directly in `if` statements:
```csharp
Result result = DoWork();
if (result) { /* Success path */ }
if (!result) { /* Failure path */ }
```

### Deconstruction
Positionally extract values:
```csharp
var (isSuccess, value, error) = result; // For Result<T>
var (isSuccess, error) = result;        // For Result
var (code, message, details) = error;   // For Error
```

### Error Aggregation
Combine multiple errors using the `+` operator:
```csharp
Error errors = Error.Validation.MissingField("Name") + 
               Error.Validation.InvalidFormat("Email");

// Result maintains all details from both errors.
```

## Chaining (Fluent API)

| Method | Finality |
|--------|----------|
| `Bind` | Chain another `Result` operation. |
| `Map` | Transform the success value. |
| `Ensure` | Validate a condition on the success value. |
| `Tap` | Execute side-effects (e.g., Logging). |
| `OnFailure` | Execute logic only on failure. |
| `Match` | Final unwrapping of success and failure cases. |

## Async Support
Use `Task<Result<T>>` for I/O bound operations. `Result<T>` provides async extension methods for seamless chaining.

```csharp
public async Task<Result<User>> GetUserAsync(int id) => 
    await _db.Users.FindAsync(id)
        .MapAsync(u => u.ToDto());
```
