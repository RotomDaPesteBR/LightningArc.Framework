---
uid: value-objects
level: 100
summary: "Guide on using immutable Value Objects in the LightningArc.Utils library to ensure domain integrity and type safety."
keywords: "Value Objects, DDD, Domain Driven Design, Immutable, Email, Validation, C#"
---

# Immutable Value Objects

Value Objects are objects whose equality is based on their value rather than a unique identity. In `LightningArc.Utils`, they are implemented as `record` types to ensure immutability and built-in structural equality.

## The Email Value Object

The `Email` type is a specialized value object that guarantees its contents always represent a valid email address.

### Key Features
- **Validation on Creation**: It is impossible to create an `Email` object with an invalid format.
- **Implicit Conversion**: Seamlessly converts to and from `string` for easy integration with standard APIs.
- **Immutability**: Once created, the value cannot be changed.

### Usage

```csharp
// Recommended creation
var email = Email.Create("user@domain.com");

// Implicit conversion
string raw = email;
Email fromString = "test@test.com"; // Validates here

// JSON Integration
// Automatically serialized as a string if EmailJsonConverter is registered.
```

## JSON Configuration

To ensure Value Objects are serialized as simple values (instead of objects with a `Value` property), register the custom converters:

```csharp
services.Configure<JsonOptions>(options => 
{
    options.SerializerOptions.Converters.AddJsonConverters();
});
```
