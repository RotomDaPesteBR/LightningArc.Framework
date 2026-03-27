---
uid: web-api
level: 100
summary: "Instructions on bridging the Result Pattern with ASP.NET Core IResult for clean, consistent Web API responses."
keywords: "Web API, ASP.NET Core, Minimal API, IResult, EndpointResult, HTTP Status Codes"
---

# Web API Integration

`LightningArc.AspNetCoreCoreCoreCore` provides tools to bridge the gap between your domain logic (using `Result`) and the HTTP layer.

## EndpointResult

`EndpointResult` is an adapter that implements `IResult`. It automatically maps the status code and message from a `Result` object to the appropriate HTTP response.

### Automatic Mapping

| Result Code | HTTP Status Code |
|-------------|------------------|
| 100 (Ok) | 200 OK |
| 101 (Created) | 201 Created |
| 102 (Accepted) | 202 Accepted |
| 103 (NoContent)| 204 No Content |
| 4XXX (Validation) | 400 Bad Request |
| 5XXX (NotFound) | 404 Not Found |
| 2XXX (Auth) | 401 Unauthorized / 403 Forbidden |

### Usage in Minimal APIs

You can return a `Result` directly by casting it to `EndpointResult`.

```csharp
app.MapGet("/users/{id}", async (int id, IUserService service) => 
{
    Result<User> result = await service.GetUser(id);
    return (EndpointResult<User>)result;
});
```

## OpenAPI (Swagger) Support

To ensure Value Objects like `Email` are displayed correctly as strings in the Swagger UI, register the schema transformers:

```csharp
builder.Services.AddOpenApi(options => 
{
    options.AddSchemaTransformers();
});
```


