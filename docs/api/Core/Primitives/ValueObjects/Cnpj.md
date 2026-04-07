# Cnpj

**Namespace:** `LightningArc.Primitives.ValueObjects`
**Type:** `record`

Represents a validated Brazilian CNPJ (Cadastro Nacional da Pessoa Jurídica) number. This type ensures immutability, validates structure.

---

## Properties

### Value
- **Type:** `string`
- **Description:** The CNPJ number formatted as `XX.XXX.XXX/XXXX-XX`.

---

## Methods

### Create(string value)
- **Static:** Yes
- **Description:** Validates the input string against CNPJ format and check digits.
- **Parameters:**
  - `value` (`string`): The raw or formatted CNPJ string.
- **Returns:** A new `Cnpj` instance.
- **Exceptions:**
  - `ArgumentException`: Thrown if the value is invalid (wrong length, format, or failed check digits).

### ToString()
- **Description:** Returns the CNPJ formatted form.
- **Returns:** `string`

---

## Operators

### implicit operator string(Cnpj cnpj)
- **Description:** Allows implicit conversion from a `Cnpj` object to a `string`.

### implicit operator Cnpj(string value)
- **Description:** Allows implicit conversion from a `string` to a `Cnpj` object, triggering validation.

---

## Usage Examples

```csharp
using LightningArc.Primitives.ValueObjects;

// Direct creation
var cnpj = Cnpj.Create("12.345.678/0001-95");

// Implicit conversion
Cnpj fromString = "12.345.678/0001-95";

// Handling invalid input
if (Cnpj.TryCreate("00.000.000/0001-91", out var validCnpj))
{
    // validCnpj is guaranteed valid
}

// ToResult for fluent handling
Result<Cnpj> result = "invalid".CreateCnpjResult();
```