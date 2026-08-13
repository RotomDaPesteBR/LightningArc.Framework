# Result Pattern

The **`LightningArc.Results`** library implements the **Result Pattern** for explicit and structured management of successes and failures. Its goal is to promote clean, expressive, and robust code by eliminating excessive reliance on exceptions for normal control flow.

---

## 1. Result Types

| Type | Usage | Description |
| :--- | :--- | :--- |
| **`Result<TValue>`** | Return with value | Represents the result of an operation containing a success value (`TValue`) or a failure (`Error`). |
| **`Result`** | Return without value | Intended for operations that only return a status (success/failure). |
| **`Error`** | Failure | A strongly-typed base class for structuring failure reasons, including numeric codes and details (`ErrorDetail`). |
| **`Success`** | Success | Base for standardized success results. |

---

## 2. Ergonomics and Syntactic Sugar

The library leverages modern C# features to make working with results as natural as possible.

### 2.1. Boolean Operators

You can use `Result` objects directly in conditional expressions:

```csharp
Result result = DoWork();

if (result) { /* Success */ }
if (!result) { /* Failure */ }
```

### 2.2. Deconstruction

Extract result values positionally:

```csharp
// For Result<T>
var (isSuccess, value, error) = result;

// For Error
var (code, message, details) = error;
```

### 2.3. Safe Access (TryGetValue / TryGetError)

Access values or errors safely without exceptions:

```csharp
Result<int> result = DoWork();

if (result.TryGetValue(out int value))
{
    // Success, 'value' contains the result
}

Result failureResult = DoSomethingElse();
if (failureResult.TryGetError(out Error error))
{
    // Failure, 'error' contains the reason
}
```

### 2.4. Explicit Casts

Access the value or error from a `Result<T>` via casting (throws if the state is invalid):

```csharp
int val = (int)result;       // Extracts the value
Error err = (Error)result;   // Extracts the error
```

---

## 3. Flow Composition (LINQ-Style)

Use extension methods to chain operations without nested `if` blocks:

| Method | Purpose |
| :--- | :--- |
| **`Bind`** | Chains operations that also return `Result`. Stops at the first error. |
| **`Map`** | Transforms a success value into another type. |
| **`Tap`** | Executes a side effect (e.g., logging) only on success. |
| **`OnFailure`** | Executes an action only on failure. |
| **`Match`** | Reduces the result to a single value by handling both success and failure cases. |

---

## 4. Error Management (`Error`)

### 4.1. Code Structure

The error code is composed of a **Prefix** (module) and a **Suffix** (specific error):
`Code = (Prefix * 1000) + Suffix`.

### 4.2. Error Aggregation (`AggregateError`)

Combine multiple errors using the `+` operator. Useful for accumulating validation failures:

```csharp
Error finalError = error1 + error2 + error3;

if (finalError is AggregateError agg)
{
    var allErrors = agg.Errors;   // Individual error list
    var flat = agg.Flatten();     // Flattens nested aggregate hierarchies
}
```

### 4.3. Error Details (`ErrorDetail`)

Add field-specific context to errors:

```csharp
return Error.Validation.InvalidParameter("Invalid data", [
    ("Name", "Required"),
    ("Age", "Must be greater than 18")
]);
```

---

## 5. Success Types

### 5.1. Success Codes

Each success type has a numeric code following the `1xx` convention:

| Method | Code | Type |
| :--- | :--- | :--- |
| **`Success.Ok()`** | 100 | Non-generic success |
| **`Success.Created()`** | 101 | Non-generic success |
| **`Success.Accepted()`** | 102 | Non-generic success |
| **`Success.NoContent()`** | 103 | Non-generic success |

### 5.2. Success State (`SuccessState`)

Both `Result` and `Result<TValue>` expose the underlying success metadata through the `SuccessState` property:

```csharp
Result result = Result.Created();
var successState = result.SuccessState; // Returns Success (code 101, message "Created")

Result<User> typedResult = Result.Success(user);
var typedSuccessState = typedResult.SuccessState; // Returns Success<User> (code 100, value: user)
```

### 5.3. Typed Success Values

Create typed success results using `Result.Success<TValue>` factories:

```csharp
Result<User> userSuccess = Result.Success(user);        // Ok (100)
Result<User> userCreated = Result.Created(user);        // Created (101)
Result<User> userAccepted = Result.Accepted(user);      // Accepted (102)
```

The `WithValue<TValue>` method on `Result` converts a non-generic success result to a typed one:

```csharp
Result ok = Result.Ok();
Result<User> typed = ok.WithValue(user);
```

### 5.4. Pattern Matching with `Match`

The `Match` method reduces a result to a single value by handling both cases, preserving the `Success<TValue>`:

```csharp
string message = result.Match(
    success => $"Created user {success.Value.Id} (code: {success.SuccessState.Code})",
    error => $"Failed: {error.Message}"
);
```

---

## 6. ASP.NET Core Integration

To convert `Result` objects into standardized HTTP responses (RFC 7807), use the **`LightningArc.Results.AspNetCore`** package:

```csharp
[HttpGet("{id}")]
public EndpointResult<User> Get(int id)
{
    return _service.GetUser(id);
    // Returns 200 OK with the User or 4xx/5xx based on the Error.
}
```

The `EndpointResult<TValue>` adapter implements `IResult` and supports implicit conversion from `Result<TValue>`.

---

## 7. Analyzers

LightningArc includes a Roslyn analyzer that warns about:

- **Unsafe access** to `Result.Value` (use `TryGetValue` or pattern matching instead).
- **Unsafe access** to `Result.Error` (use `TryGetError` or pattern matching instead).
- **Implicit string-to-ValueObject conversion** pitfalls.

For details, see [Analyzers](../analyzers/README.md).