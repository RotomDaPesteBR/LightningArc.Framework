# ValueObject Rules

Rules LARC010–LARC012 enforce explicit handling of ValueObject creation and string conversions.

---

## LARC010

**Implicit string → ValueObject conversion may throw**

ValueObjects like `Email`, `Cpf`, `Cnpj`, `PhoneNumber`, and `Url` have implicit conversions from `string` that throw on invalid input. This rule flags those conversions so you are aware of the exception risk.

### ❌ Bad

```csharp
Email email = "invalid-email"; // throws ArgumentException
```

### ✅ Good

```csharp
// Option 1: Use TryCreate
Email.TryCreate("user@example.com", out var email);

// Option 2: Use Result extensions
var result = "invalid-email".AsEmail();
if (result.IsFailure) { /* handle */ }

// Option 3: Use Create explicitly (knowing it throws)
var email = Email.Create("user@example.com");
```

---

## LARC011

**Potential null ValueObject → string conversion may throw**

Null reference ValueObjects can throw `NullReferenceException` when implicitly converted to `string`.

### ❌ Bad

```csharp
Email? email = null;
string s = email; // NullReferenceException
```

### ✅ Good

```csharp
string s = email?.Value ?? string.Empty;
// or
string s = email?.ToString() ?? string.Empty;
```

---

## LARC012

**ValueObject creation result discarded**

Calling `Create()` or `TryCreate()` on a ValueObject without capturing or checking the result means the work is wasted.

### ❌ Bad

```csharp
Email.Create("user@example.com"); // Result ignored
Email.TryCreate("user@example.com", out _); // Discarded
```

### ✅ Good

```csharp
var email = Email.Create("user@example.com"); // used
Email.TryCreate("user@example.com", out var email); // captures result
```
