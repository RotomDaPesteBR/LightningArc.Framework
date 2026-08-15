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

**Static batch combinator:** For combining a collection of errors in a single call (avoids repeated re-flattening from `+=`):

```csharp
// Combine an array/list of errors
Error finalError = Error.Aggregate(errors);

// Conditional aggregation — null entries are ignored
Error finalError = Error.Aggregate(
    isValid ? null : Error.Validation.MissingField("name"),
    isUnique ? null : Error.Validation.InvalidParameter("email", "Already in use"),
    hasPermission ? null : Error.Authentication.Forbidden("access")
);

// Result semantics:
// - Empty or all null → null
// - Single error → that error (unwrapped, not wrapped in AggregateError)
// - Multiple same code → AggregateError preserving that code
// - Multiple mixed codes → AggregateError with general code (99001)
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

## 7. Validation Aggregation (`ResultAggregator`)

The **`ResultAggregator`** provides a fluent builder for collecting multiple validation results without short-circuiting on the first failure. This is ideal for validation scenarios where you want to report all errors at once.

### 7.1. Basic Usage

```csharp
var aggregator = Result.Aggregate()
    .Check(() => ValidateName(name))
    .Check(() => ValidateEmail(email))
    .Check(() => ValidateAge(age));

Result result = aggregator.Build();

if (result.IsFailure)
{
    // Contains all aggregated errors
    var error = result.Error;
}
```

### 7.2. Batch Checks

Execute multiple checks from a collection:

```csharp
var checks = new Func<Result>[]
{
    () => ValidateField1(),
    () => ValidateField2(),
    () => ValidateField3()
};

var aggregator = Result.Aggregate()
    .Check(checks);       // Sequential execution
```

`CheckEach` is an alias for `Check(IEnumerable<...>)` with an explicit name indicating batch semantics — use whichever reads better at the call site:

```csharp
var aggregator = Result.Aggregate()
    .CheckEach(checks);   // Same as .Check(checks)
```

### 7.3. Capture Values

Capture successful values during aggregation (callback-based for async compatibility):

```csharp
var aggregator = Result.Aggregate()
    .Check(CreateUser, user => { /* use user */ })  // Callback-based, works with async
    .Check(CreateProfile, profile => { /* use profile */ });

Result result = aggregator.Build();
// Use captured values via callbacks if result.IsSuccess
```

**Note:** The `out` parameter overload (`Check<T>(factory, out T? value)`) is synchronous only and cannot be used after an awaited step in the chain.

### 7.4. Condition Checks

Add ad-hoc conditions with custom errors:

```csharp
var aggregator = Result.Aggregate()
    .Ensure(age >= 18, Error.Validation.InvalidParameter("Age", "Must be 18+"))
    .Ensure(email.Contains("@"), Error.Validation.InvalidParameter("Email", "Invalid format"));
```

### 7.5. Async Support

```csharp
Result result = await Result.Aggregate()
    .CheckAsync(() => ValidateEmailUniqueAsync(email))
    .CheckAsync(() => ValidateUsernameUniqueAsync(username))
    .BuildAsync();
```

### 7.6. Concurrent Batch Execution

Run multiple async checks concurrently:

```csharp
var asyncChecks = new Func<Task<Result>>[]
{
    () => ValidateEmailUniqueAsync(email),
    () => ValidateUsernameUniqueAsync(username),
    () => ValidatePhoneUniqueAsync(phone)
};

Result result = await Result.Aggregate()
    .CheckAll(asyncChecks)
    .BuildAsync();
```

### 7.7. Capture Values from Result<T> Checks

Capture values from checks that return `Result<T>` (not just `Result`):

```csharp
User? user = null;
Profile? profile = null;

Result result = await Result.Aggregate()
    .CheckAsync(() => CreateUserAsync(), u => user = u)
    .CheckAsync(() => CreateProfileAsync(), p => profile = p)
    .BuildAsync();
// user and profile available via callbacks on success
```

### 7.8. Lazy Error Factory (Ensure)

Defer error creation until the condition actually fails:

```csharp
var aggregator = Result.Aggregate()
    .Ensure(age >= 18, () => Error.Validation.InvalidParameter("Age", "Must be 18+"))
    .Ensure(email.Contains("@"), () => Error.Validation.InvalidParameter("Email", "Invalid format"));
```

The `Func<Error>` is only invoked when the condition is false, avoiding allocation for passing checks.

### 7.9. Condition Checks (When / WhenAsync / WhenAll)

The `When` family checks boolean conditions **without catching exceptions** — exceptions propagate normally. Use for preconditions and invariants.

```csharp
// Synchronous
Result result = Result.Aggregate()
    .When(() => user.IsActive, Error.Validation.InvalidState("User", "Account disabled"))
    .When(() => user.HasPermission("write"), Error.Authorization.Forbidden("write"))
    .Build();

// Asynchronous
Result result = await Result.Aggregate()
    .WhenAsync(() => CheckLicenseAsync(user.Id), Error.License.Expired())
    .WhenAsync(() => CheckQuotaAsync(user.Id), Error.Quota.Exceeded())
    .BuildAsync();
```

Run multiple async conditions concurrently:

```csharp
var conditions = new WhenCondition[]
{
    new(() => CheckLicenseAsync(user.Id), Error.License.Expired()),
    new(() => CheckQuotaAsync(user.Id), Error.Quota.Exceeded()),
    new(() => CheckRegionAsync(user.Id), Error.Region.Unavailable())
};

Result result = await Result.Aggregate()
    .WhenAll(conditions)
    .BuildAsync();  // Concurrent via Task.WhenAll, exceptions propagate
```

### 7.10. Concurrent Batch with Per-Check Callbacks (AsyncCheck)

Run checks concurrently with a callback fired **immediately** when each check completes (not after the batch):

```csharp
var checks = new AsyncCheck[]
{
    new(() => ValidateEmailUniqueAsync(email), r => LogCheck(r, "email")),
    new(() => ValidateUsernameUniqueAsync(username), r => LogCheck(r, "username")),
    new(() => ValidatePhoneUniqueAsync(phone), r => LogCheck(r, "phone"))
};

Result result = await Result.Aggregate()
    .CheckAll(checks)
    .BuildAsync();
// Each callback fired as its check resolved, concurrently
```

**Behavior notes:**
- Callbacks fire on arbitrary threads (no `ConfigureAwait(true)`)
- `Action` (check) exceptions are caught and converted to failures
- `OnComplete` (callback) exceptions **propagate** and fault the batch (asymmetry by design)
- Implicit conversion: `IEnumerable<Func<Task<Result>>>` still works — bare delegates become `AsyncCheck` with no callback

### 7.11. Task Extensions (Fluent Async Chaining)

For fully fluent async workflows, use the `Task<ResultAggregator>` extension methods:

```csharp
Result result = await Result.Aggregate()
    .CheckAsync(() => ValidateEmailAsync(email))
    .CheckAll(asyncChecks)
    .Ensure(condition, error)
    .BuildAsync();
```

All sync and async `Check`/`Ensure`/`When`/`CheckAll` methods have corresponding `Task<ResultAggregator>` extensions so the fluent chain never breaks across sync/async boundaries.

### 7.12. Error Aggregation Behavior

- Errors are collected in a list and combined via `Error.Aggregate` in `Build()`/`BuildAsync()`
- Single error: returned directly (unwrapped)
- Multiple errors with same code: `AggregateError` preserving that code
- Multiple errors with different codes: `AggregateError` with general code (99001)
- `AggregateError.FlattenedErrors` provides all leaf errors recursively

---

## 8. Analyzers

LightningArc includes a Roslyn analyzer that warns about:

- **Unsafe access** to `Result.Value` (use `TryGetValue` or pattern matching instead).
- **Unsafe access** to `Result.Error` (use `TryGetError` or pattern matching instead).
- **Implicit string-to-ValueObject conversion** pitfalls.

For details, see [Analyzers](../analyzers/README.md).