# Password

**Namespace:** `LightningArc.Primitives.ValueObjects`
**Type:** `record`

Represents a validated password with strength assessment. This type ensures immutability, enforces minimum complexity rules, and classifies the password strength.

---

## Properties

### Value
- **Type:** `string`
- **Description:** The raw password value (masked as asterisks in `ToString()`).

### Strength
- **Type:** `PasswordStrength`
- **Description:** The assessed strength of the password (`Weak`, `Moderate`, or `Strong`).

---

## Methods

### Create(string value, PasswordStrength minimumStrength = PasswordStrength.Moderate)
- **Static:** Yes
- **Description:** Validates the password against minimum complexity rules and the specified strength level.
- **Parameters:**
  - `value` (`string`): The plain-text password.
  - `minimumStrength` (`PasswordStrength`): The minimum required strength level (default: `Moderate`).
- **Returns:** A new `Password` instance.
- **Exceptions:**
  - `ArgumentException`: Thrown if the password does not meet the minimum strength requirement.

### TryCreate(string value, out Password? result)
- **Static:** Yes
- **Description:** Attempts to create a `Password` without throwing exceptions (uses default `Moderate` minimum).
- **Parameters:**
  - `value` (`string`): The plain-text password.
  - `result` (`out Password?`): The created instance if valid, or `null`.
- **Returns:** `bool` — `true` if the password meets requirements; otherwise, `false`.

### ToString()
- **Description:** Returns a masked representation (asterisks matching password length) for safe logging.
- **Returns:** `string`

---

## Usage Examples

```csharp
using LightningArc.Primitives.ValueObjects;

// Direct creation with default minimum strength (Moderate)
var password = Password.Create("MyP@ssw0rd!");
Console.WriteLine(password.Strength); // Strong
Console.WriteLine(password.ToString()); // ***********

// TryCreate for safe handling
if (Password.TryCreate("Secur3P@ss", out var validPassword))
{
    // validPassword is guaranteed strong enough
}

// AsPassword for fluent handling
Result<Password> result = "weak".AsPassword();

// Implicit conversion to Result<Password>
Result<Password> wrapped = password;
```

---

## Related Types

### PasswordStrength
**Enum:** `Weak`, `Moderate`, `Strong`

Classifies the assessed strength of a password based on length, character variety, and special character presence.
