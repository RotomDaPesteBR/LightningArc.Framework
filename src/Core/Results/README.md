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

## Working with Results

```csharp
Result<User> result = CreateUser("test@email.com", "12345678");

if (result)
{
    Console.WriteLine(result.Value);
}
else
{
    Console.WriteLine(result.Error.Message);
}
```

---

## Error Aggregation

```csharp
var result = Result.Aggregate()
    .Check(() => ValidateEmail(email))
    .Check(() => ValidatePassword(password))
    .Build();

if (!result)
{
    foreach (var detail in result.Error.Details)
    {
        Console.WriteLine($"{detail.Context}: {detail.Message}");
    }
}
```

---

## Strongly Typed Success

```csharp
return Result.Created(user);
```

---

## Async Support

```csharp
TaskResult<User> result = userService.CreateAsync();

var userResult = await result;

if (userResult)
{
    Console.WriteLine(userResult.Value);
}
```

---

## Error Model

Errors are structured and composable:

* Numeric code (prefix + suffix)
* Message (localized or literal)
* Optional details
* Aggregation support

---

## When to Use

Use LightningArc.Results when you want:

* Consistent error handling across your application
* Explicit success semantics instead of booleans
* Validation without exceptions
* Structured domain errors
* Predictable control flow

---

## When NOT to Use

* Extremely simple applications
* Scripts or throwaway code
* When exceptions are already the dominant pattern

---

## Integration

For ASP.NET Core integration, see:

👉 LightningArc.Results.AspNetCore
