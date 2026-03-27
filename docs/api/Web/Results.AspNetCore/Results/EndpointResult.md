# EndpointResult

**Namespace:** `LightningArc.Results.AspNetCoreCore`  
**Type:** `class`

Acts as an adapter between the business logic `Result` and the ASP.NET Core `IResult`. It allows Minimal APIs or Controllers to return `Result` objects directly, which are then mapped to appropriate HTTP status codes and response bodies.

---

## EndpointResult (Non-Generic)

### Methods

*   **ExecuteAsync(HttpContext httpContext)**: Implements `IResult`. Executes the internal HTTP result.

### Operators

*   **implicit operator EndpointResult(Result result)**: Automatically converts a `Result` into an `IResult` response.
    *   **Success:** Maps to a `SuccessResult` (e.g., 200 OK, 201 Created).
    *   **Failure:** Maps to an `ErrorResult` (e.g., 400 Bad Request, 404 Not Found).

---

## EndpointResult<TValue> (Generic)

### Static Methods

*   **FromResult(Result<TValue> result, string? contentType)**: Creates an `EndpointResult` with a specific content type.

### Operators

*   **implicit operator EndpointResult<TValue>(Result<TValue> result)**: Converts a generic result into an HTTP response.
*   **implicit operator EndpointResult<TValue>(TValue value)**: Directly converts a value into a 200 OK response.
*   **implicit operator EndpointResult<TValue>(Error error)**: Directly converts an `Error` into an error response.

---

## Usage Example (Minimal API)

```csharp
app.MapGet("/users/{id}", (int id, IUserService service) => {
    Result<User> result = service.GetUser(id);
    
    // Automatic conversion from Result<User> to IResult
    return (EndpointResult<User>)result; 
});
```

