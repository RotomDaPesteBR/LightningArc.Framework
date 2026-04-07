# Rg

**Namespace:** `LightningArc.Primitives.ValueObjects`
**Type:** `record`

Represents a validated Brazilian RG (Registro Geral) number. This type ensures immutability and validates the format with optional verification character (digit or X).

---

## Properties

### Value
- **Type:** `string`
- **Description:** The RG number in cleaned format.

---

## Methods

### Create(string value)
- **Static:** Yes
- **Description:** Validates the input string against the RG format.
- **Parameters:**
  - `value` (`string`): The raw or formatted RG string (accepts dots, hyphens, and optional X check character).
- **Returns:** A new `Rg` instance.
- **Exceptions:**
  - `ArgumentException`: Thrown if the value is invalid (wrong format).

### TryCreate(string value, out Rg? result)
- **Static:** Yes
- **Description:** Attempts to create an `Rg` without throwing exceptions.
- **Parameters:**
  - `value` (`string`): The raw RG string.
  - `result` (`out Rg?`): The created instance if valid, or `null`.
- **Returns:** `bool` — `true` if the RG is valid; otherwise, `false`.

### ToString()
- **Description:** Returns the RG string.
- **Returns:** `string`

---

## Operators

### implicit operator string(Rg rg)
- **Description:** Allows implicit conversion from an `Rg` object to a `string`.

### implicit operator Rg(string value)
- **Description:** Allows implicit conversion from a `string` to an `Rg` object, triggering validation.

---

## Usage Examples

```csharp
using LightningArc.Primitives.ValueObjects;

// Direct creation
var rg = Rg.Create("12.345.678-9");
var rg2 = Rg.Create("12345678X");

// TryCreate for safe handling
if (Rg.TryCreate("12.345.678-0", out var validRg))
{
    // validRg is guaranteed valid
}

// ToResult for fluent handling
Result<Rg> result = "invalid".CreateRgResult();
```
