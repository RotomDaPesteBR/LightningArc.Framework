# Value Objects

Value Objects are immutable record types with built-in validation. They eliminate Primitive Obsession by ensuring domain values are always valid at the type level.

---

## Core Value Objects

### Email

The `Email` type ensures a valid email format.

#### Creation

```csharp
using LightningArc.Primitives.ValueObjects;

try
{
    var userEmail = Email.Create("user@example.com");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid email: {ex.Message}");
}
```

#### Implicit Conversion

```csharp
// CAUTION: Throws ArgumentException if the string is invalid.
Email myEmail = "contact@site.com";

// To string
string emailString = myEmail;

// Or explicitly
Console.WriteLine(myEmail.Value);
```

#### Equality

`Email` is a `record`, so two instances with the same address are equal:

```csharp
var email1 = Email.Create("john@doe.com");
var email2 = Email.Create("john@doe.com");

if (email1 == email2)
{
    Console.WriteLine("Emails are equal.");
}
```

### Cpf

The `Cpf` type validates a Brazilian CPF (taxpayer ID).

```csharp
var cpf = Cpf.Create("12345678909");
```

### Cnpj

The `Cnpj` type validates a Brazilian CNPJ (corporate tax ID).

```csharp
var cnpj = Cnpj.Create("12345678000195");
```

### PhoneNumber

The `PhoneNumber` type validates phone numbers.

```csharp
var phone = PhoneNumber.Create("+5511999999999");
```

### Url

The `Url` type validates URL strings.

```csharp
var url = Url.Create("https://example.com/path");
```

### Rg

The `Rg` type validates a Brazilian RG (identity card number).

```csharp
var rg = Rg.Create("123456789");
```

### Currency

The `Currency` type validates currency values.

```csharp
var amount = Currency.Create(99.99m);
```

### Cep

The `Cep` type validates a Brazilian postal code (CEP).

```csharp
var cep = Cep.Create("01001000");
```

---

## TryCreate Pattern

All value objects support a non-throwing `TryCreate` pattern:

```csharp
if (Email.TryCreate("user@example.com", out var email))
{
    // Success - use 'email'
}
else
{
    // Invalid input - no exception thrown
}
```

---

## Implementation Details

For source-level details, see the [internal implementation docs](../advanced/internals/Core/ValueObjects).