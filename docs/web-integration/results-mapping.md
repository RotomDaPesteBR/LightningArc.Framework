# Results.AspNetCore

The **`LightningArc.Results.AspNetCore`** library provides a bridge between your domain layer (`Result<T>`) and ASP.NET Core, automatically converting results to standardized HTTP responses (RFC 7807 Problem Details).

---

## 1. Endpoint Adaptors (`EndpointResult`)

The `EndpointResult` and `EndpointResult<TValue>` types eliminate boilerplate when converting `Result` objects into `IResult` for ASP.NET Core controllers.

```csharp
[HttpGet("{id}")]
public async Task<EndpointResult<Product>> Get(int id)
{
    // Returns Result<Product> directly.
    // The adaptor converts to 200 OK or 4xx/5xx based on the result.
    return await _service.GetProduct(id);
}
```

### Automatic Behavior
- **Success**: Maps to the HTTP code matching the success type (e.g., `Ok` → 200, `Created` → 201).
- **Failure**: Converts the `Error` object into a Problem Details response with the appropriate status code.

---

## 2. Global Exception Handling

The library includes `ResultExceptionHandler`, which captures unhandled exceptions and converts them into standardized `Error` objects, ensuring your API never returns a raw stack trace.

### Setup
In `Program.cs` (.NET 9.0+):

```csharp
builder.Services.AddEndpointResults(); // Also registers the ExceptionHandler
// ...
app.UseExceptionHandler(); // Activates the ASP.NET Core exception pipeline
```

### Default Exception Mappings
| Exception | Result Error | HTTP Status |
| :--- | :--- | :--- |
| `ValidationException` | `Validation.InvalidParameter` | 400 Bad Request |
| `UnauthorizedAccessException` | `Authentication.Forbidden` | 403 Forbidden |
| `DbException` | `Database.ConnectionFailed` | 500 Internal Server Error |
| `NotImplementedException` | `Application.NotImplemented` | 501 Not Implemented |

---

## 3. Mapping Configuration

You can centralize the translation between domain errors and HTTP contracts:

```csharp
builder.Services.AddEndpointResults(configureMappings: (successes, errors) =>
{
    errors.Map<MyDomainError>(HttpStatusCode.Conflict, "Error Title", "urn:api:error-type");
});
```

---

## 4. Automatic Error Documentation

Keep your API documentation up to date with your domain's error vocabulary:

```csharp
if (app.Environment.IsDevelopment())
{
    // Generates a Markdown file listing all known errors and their HTTP mappings
    app.OutputErrorsList("Docs/Errors.md");
}
```