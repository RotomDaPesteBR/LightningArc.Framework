# LightningArc.Results

LightningArc.Results is an opinionated Result library for .NET focused on structured domain errors, explicit success semantics, and predictable application flows.

Instead of relying on exceptions for control flow or using loosely structured error messages, it models failures as first-class objects with codes, messages, and contextual details.

---

## Why LightningArc.Results?

LightningArc.Results is designed for applications that need more than a simple `Result<T>` wrapper.

It provides:

* Structured errors with numeric codes and contextual details
* Explicit success semantics (`Ok`, `Created`, `Accepted`, `NoContent`)
* Strong typing for both success and failure flows
* Error aggregation for validation scenarios
* Exception-to-error conversion support
* Localization support for messages
* Fully framework-agnostic design

If you only need a minimal success/failure abstraction, simpler libraries may be sufficient.
If you need consistency, structure, and domain-oriented error modeling, LightningArc.Results is built for that.

---

## Quick Example

```csharp
using LightningArc.Results;

public Result<User> CreateUser(string email, string password)
{
    if (string.IsNullOrEmpty(email))
        return Error.Validation.MissingField("Email");

    if (password.Length < 8)
        return Error.Validation.ValueOutOfRange("Password");

    return Result.Success(new User(email, password));
}
```

---

## Core Types

| Type | Purpose |
|------|---------|
| `Result` | Non-generic result for operations without a return value |
| `Result<TValue>` | Generic result containing a success value or error |
| `Error` | Structured failure type with code, message, and optional details |
| `Success` / `Success<TValue>` | Strongly typed success states carrying code, message, and, for generic successes, a value |

---

## Working with Results

### Safe Access (Preferred)

```csharp
Result<User> result = CreateUser("test@email.com", "12345678");

if (result.TryGetValue(out var user))
{
    Console.WriteLine(user);
}
else
{
    Console.WriteLine(result.Error.Message);
}
```

### Boolean Operators

```csharp
Result result = DoWork();

if (result) { /* Success */ }
if (!result) { /* Failure */ }
```

### Deconstruction

```csharp
// For Result<T>
var (isSuccess, value, error) = result;

// For Error
var (code, message, details) = error;
```

### Pattern Matching

```csharp
string message = result.Match(
    success => $"Created user {success.Value.Id} (code: {success.SuccessState.Code})",
    error => $"Failed: {error.Message}"
);
```

---

## Flow Composition (LINQ-Style)

Chain operations without nested `if` blocks:

| Method | Purpose |
|--------|---------|
| `Bind` | Chain operations that return `Result`; stops at first error |
| `Map` | Transform a success value into another type |
| `Tap` | Execute side effect (e.g., logging) only on success |
| `OnFailure` | Execute action only on failure |
| `Match` | Reduce to a single value handling both cases |

```csharp
return Email.Create(email)
    .Bind(e => _repo.AddAsync(e))
    .Map(u => u.ToDto());
```

---

## Error Management

### Code Structure

Error code = `Prefix * 1000 + Suffix` (module + specific error)

```csharp
public class Ordering : Error.ErrorModule
{
    public new const int CodePrefix = 12;

    public class InventoryInsufficientError : Error
    {
        internal InventoryInsufficientError(string message, List<ErrorDetail>? details = null)
            : base(CodePrefix, 01, message, details) { }
    }
}
```

### Aggregation

Combine multiple errors using `+` operator:

```csharp
Error? errors = null;
if (string.IsNullOrEmpty(name))
    errors += Error.Validation.MissingField("Name");
if (age < 18)
    errors += Error.Validation.ValueOutOfRange("Age");

return errors != null ? errors : Result.Success();
```

### Fluent Aggregation (`Result.Aggregate`)

Collect multiple validation results without short-circuiting:

```csharp
var result = Result.Aggregate()
    .Check(() => ValidateName(name))
    .Check(() => ValidateAge(age))
    .Build();
```

`ResultAggregator` also captures values on success and converts exceptions to errors.

### Details

Add field-specific context:

```csharp
return Error.Validation.InvalidParameter("Invalid data", [
    ("Name", "Required"),
    ("Age", "Must be greater than 18")
]);
```

### Domain Extensions

```csharp
// Access domain error module
var error = Error.Of<Ordering>().InventoryInsufficient("Low stock", 
    [new ErrorDetail("ProductId", "SKU-123")]);

// Or via extension (modern C#)
var error = Error.Ordering.InventoryInsufficient();
```

---

## Success Types

### Success Codes (1xx Convention)

| Method | Code | Description |
|--------|------|-------------|
| `Success.Ok()` | 100 | Standard success |
| `Success.Created()` | 101 | Resource created |
| `Success.Accepted()` | 102 | Accepted for processing |
| `Success.NoContent()` | 103 | Success with no content |

### Typed Success Values

```csharp
Result<User> userSuccess = Result.Success(user);        // Ok (100)
Result<User> userCreated = Result.Created(user);        // Created (101)
Result<User> userAccepted = Result.Accepted(user);      // Accepted (102)
```

### SuccessState

`SuccessState` carries the full success behavior — code, message, and, for generic results, the value itself. It represents the concrete success type and provides the information used by downstream consumers such as HTTP mapping.

```csharp
Result result = Result.Created();
var successState = result.SuccessState; // Success (code 101, "Created")

Result<User> typedResult = Result.Success(user);
var typedSuccessState = typedResult.SuccessState; // Success<User> (code 100, value: user)
```

### Converting Between Generic/Non-Generic

```csharp
Result ok = Result.Ok();
Result<User> typed = ok.WithValue(user);
```

---

## Extensibility

Create domain-specific extensions using the modern extension member pattern (C# 14 / .NET 10+) with `Of`/`Hook` compatibility for older targets.

### Success Extensions

```csharp
public class OrderConfirmedSuccess : Success
{
    internal OrderConfirmedSuccess() : base(101, "Order confirmed") { }
}

// Modern extension member syntax (C# 14 / .NET 10+)
extension(Success)
{
    public static Result OrderConfirmed(this Success.Hook _)
        => new OrderConfirmedSuccess();
}

// Usage — modern (preferred on net10+)
var result = Result.OrderConfirmed();

// Usage — compatibility (works on netstandard2.0/net9.0)
var result = Result.Of.OrderConfirmed();
```

### Error Extensions

```csharp
extension(Error)
{
    public static Error.ErrorModule<Ordering> Ordering => Error.Of<Ordering>();
}

// Usage — modern (preferred on net10+)
var error = Error.Ordering.InventoryInsufficient();

// Usage — compatibility
var error = Error.Of<Ordering>().InventoryInsufficient();
```

---

## Implicit Conversions

```csharp
// Error to Result
Result result = Error.Validation.MissingField("Email");

// Value to Result<T>
Result<User> result = new User("test@email.com", "password");

// Success to Result
Result result = Result.Success();
```

---

## Exception Mapping

Convert exceptions to structured `Error` objects automatically using the built-in exception mapper:

```csharp
using LightningArc.Results.Exceptions;

try
{
    await _repository.SaveAsync(entity);
}
catch (Exception ex)
{
    return ex.ToError(); // Maps to appropriate Error type based on exception
}
```

`Exception.ToError()` is the convenient extension API; `ExceptionMapper` can be used directly when explicit mapping is preferred:

```csharp
Error error = ExceptionMapper.Map(ex);
```

Default mappings cover common .NET exceptions:
- `ArgumentNullException` → `Error.Validation.MissingField`
- `ArgumentOutOfRangeException` → `Error.Validation.ValueOutOfRange`
- `UnauthorizedAccessException` → `Error.Authentication.Forbidden`
- `TimeoutException` → `Error.Network.RequestTimeout`
- `DbException` → `Error.Database.QueryExecutionFailed`
- `KeyNotFoundException` → `Error.Resource.NotFound`
- And many more...

Register custom mappings at startup:

```csharp
ExceptionMapper.Register<MyCustomException>(ex =>
    Error.Custom<MyDomain>().CustomError(ex.Message));
```

---

## Localization

Configure culture and custom resource managers for error/success messages:

```csharp
using System.Reflection;
using System.Resources;
using LightningArc.Results.Localization;

// At application startup (Program.cs)
LocalizationManager.Configure("pt-BR");

// Or with custom resource managers
LocalizationManager.Configure(
    "en-US",
    errorResourceManager: new ResourceManager("MyApp.Resources.Errors", Assembly.GetExecutingAssembly()),
    successResourceManager: new ResourceManager("MyApp.Resources.Success", Assembly.GetExecutingAssembly())
);
```

The library defaults to `CultureInfo.InvariantCulture` until explicitly configured. All built-in messages support localization via resource keys.

---

## ASP.NET Core Integration

Use `LightningArc.Results.AspNetCore` for automatic HTTP mapping (RFC 7807 Problem Details):

```csharp
[HttpGet("{id}")]
public EndpointResult<User> Get(int id)
{
    return _service.GetUser(id);
    // Returns 200 OK with User or 4xx/5xx based on Error
}
```

```csharp
// Program.cs
builder.Services.AddEndpointResults();
app.UseExceptionHandler();
```

---

## Analyzers

Roslyn analyzers covering Result safety, ValueObject usage, ADO.NET patterns, and Minimal API mapping, plus automatic code fixes. They warn about:

* Unsafe `Result.Value` access (use `TryGetValue` or pattern matching)
* Unsafe `Result.Error` access (use `TryGetError` or pattern matching)
* Implicit string-to-ValueObject conversion pitfalls

LightningArc packages automatically include the relevant analyzers as transitive dependencies. Consumers do not normally need to install `LightningArc.Analyzers` separately.

The analyzer package can also be installed explicitly when consuming it independently:

```bash
dotnet add package LightningArc.Analyzers
```

The analyzer package is configured with `EnforceExtendedAnalyzerRules` for optimal diagnostics.

---

## Target Frameworks

* `netstandard2.0`
* `net9.0`
* `net10.0`

---

## Installation

```bash
dotnet add package LightningArc.Results
```

For ASP.NET Core integration:

```bash
dotnet add package LightningArc.Results.AspNetCore
```