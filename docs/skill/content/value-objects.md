# Value Objects

Value Objects are immutable types that represent domain concepts defined by their attributes rather than a persistent identity.

## `Email` Value Object
**Namespace**: `LightningArc.Abstractions.ValueObjects`

Used to encapsulate email validation logic and ensure data integrity.

### Usage
```csharp
// Creation (throws ArgumentException on invalid format)
Email email = Email.Create("user@example.com");

// Implicit conversion from string
Email email = "user@example.com";

// Access value
string raw = email.Value;
string rawImplicit = email; // Implicit to string
```

### Features
- **Regex Validation**: Built-in validation for standard email formats.
- **Immutability**: Once created, the value cannot be changed.
- **Equality**: Two `Email` objects are equal if their `Value` strings are identical.
