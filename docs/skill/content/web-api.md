# Web API Integration

The **`LightningArc.Results.AspNetCore`** library bridges the gap between your domain logic and HTTP responses.

## `EndpointResult`
Used as a return type in Controllers or Minimal APIs to automatically map `Result` objects to HTTP Status Codes and Problem Details.

```csharp
[HttpGet]
public async Task<EndpointResult<User>> Get(int id) => 
    await _service.GetByIdAsync(id);
```

### Implicit Mappings
- `Success.Ok` -> 200 OK
- `Success.Created` -> 201 Created
- `Error.Validation.*` -> 400 Bad Request
- `Error.Resource.NotFound` -> 404 Not Found
- `Error.*` (unmapped) -> 500 Internal Server Error

## Global Exception Handling
The library provides **`ResultExceptionHandler`** to catch raw exceptions and return them as RFC 7807 Problem Details.

### Registration
```csharp
builder.Services.AddEndpointResults();
// ...
app.UseExceptionHandler();
```

## OpenAPI Support
Ensure your Swagger/OpenAPI documentation reflects the correct schema for `Email` and other types:

```csharp
builder.Services.AddOpenApi(options => {
    options.AddSchemaTransformers();
});
```
