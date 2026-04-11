# LightningArc.Results.AspNetCore

LightningArc.Results.AspNetCore provides seamless integration between LightningArc.Results and ASP.NET Core, allowing domain results to be automatically mapped to HTTP responses.

It bridges the gap between application logic and the web layer, converting Result objects into standardized HTTP responses, including RFC 7807 Problem Details.

---

## Why LightningArc.Results.AspNetCore?

This package allows you to:

* Return Result directly from endpoints
* Automatically map errors to HTTP status codes
* Generate RFC 7807 Problem Details responses
* Customize success and error responses
* Configure mappings per error type
* Handle exceptions globally and convert them into structured errors

---

## Installation

```bash
dotnet add package LightningArc.Results.AspNetCore
```

---

## Setup

```csharp
builder.Services.AddEndpointResults();
```

Optional configuration:

```csharp
builder.Services.AddEndpointResults(
    wrapSuccessResponses: true,
    configureMappings: (success, error) =>
    {
        error.Map<Error.Validation.InvalidParameterError>(
            HttpStatusCode.BadRequest,
            "Invalid parameter",
            "urn:api-errors:invalid-parameter"
        );
    }
);
```

---

## Usage in Endpoints

```csharp
app.MapPost("/users", (UserRequest req) =>
{
    return userService.Create(req);
});
```

---

## Result → HTTP Mapping

* Success → 200 / 201 / 202 / 204
* Failure → mapped via ErrorMappingService
* Default → RFC 7807 ProblemDetails

---

## Example Response (Error)

```json
{
  "type": "urn:api-errors:invalid-parameter",
  "title": "Invalid parameter",
  "status": 400,
  "detail": "Email is invalid",
  "instance": "/users"
}
```

---

## Custom Response Builders

```csharp
builder.Services.AddEndpointResults(
    wrapSuccessResponses: true,
    successResponseBuilder: (success, ctx) => new
    {
        code = success.Code,
        message = success.Message,
        data = success.Value
    }
);
```

---

## Exception Handling

```csharp
builder.Services.AddEndpointExceptionHandler();
```

---

## Error Mapping

```csharp
error.Map<MyCustomError>(
    HttpStatusCode.Conflict,
    "Business rule violation",
    "urn:api-errors:business-rule"
);
```

---

## Error List Generation

```csharp
app.OutputErrorsList();
```

---

## When to Use

Use this package when:

* Building APIs with ASP.NET Core
* You want consistent HTTP responses
* You want to eliminate manual mapping logic
* You use LightningArc.Results in your domain layer

---

## Dependency

This package requires:

👉 LightningArc.Results
