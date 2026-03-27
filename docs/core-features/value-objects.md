# Abstractions Usage Guide

The **Abstractions** library provides common base types and Value Objects used throughout the LightningArc ecosystem.

## Value Objects

### Email

The `Email` type is an immutable Value Object that guarantees a valid email format. It prevents "Primitive Obsession" by ensuring that you never have to repeatedly validate email strings in your business logic.

#### Creation

You can create an `Email` object using the static `Create` method. This method validates the input immediately.

```csharp
using LightningArc.Abstractions.ValueObjects;

// 1. Explicit Creation (Recommended)
try 
{
    var userEmail = Email.Create("user@example.com");
    // 'userEmail' is now guaranteed to be valid.
}
catch (ArgumentException ex)
{
    // Handle invalid email format
    Console.WriteLine($"Invalid email: {ex.Message}");
}
```

#### Implicit Conversion

The type supports implicit conversion from and to `string`, allowing for cleaner syntax.

**From String:**
```csharp
// CAUTION: This will throw an ArgumentException if the string is invalid.
Email myEmail = "contact@site.com"; 
```

**To String:**
```csharp
Email myEmail = Email.Create("test@domain.com");

// Implicitly converts to string
string emailString = myEmail; 

// Or explicitly
Console.WriteLine(myEmail.Value);
```

#### Equality

Since `Email` is a C# `record`, two different instances with the same email string are considered equal.

```csharp
var email1 = Email.Create("john@doe.com");
var email2 = Email.Create("john@doe.com");

if (email1 == email2) 
{
    Console.WriteLine("Emails are equal."); // This will print.
}
```

---

## Implementation Details

For deep dives into the source code, see:
*   [Abstractions Implementation](../advanced/internals/Core/Abstractions/README.md)
*   [Email.cs](../advanced/internals/Core/Abstractions/ValueObjects/Email.md)