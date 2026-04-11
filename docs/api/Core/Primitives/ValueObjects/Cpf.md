# Cpf

**Namespace:** `LightningArc.Primitives.ValueObjects`
**Type:** `record`

Represents a validated Brazilian CPF (Cadastro de Pessoas Físicas) number. This type ensures immutability, validates the format (11 digits), and verifies the check digits.

---

## Properties

### Value
- **Type:** `string`
- **Description:** The CPF number formatted as `XXX.XXX.XXX-XX`.

---

## Methods

### Create(string value)
- **Static:** Yes
- **Description:** Validates the input string against CPF format and check digits.
- **Parameters:**
  - `value` (`string`): The raw or formatted CPF string.
- **Returns:** A new `Cpf` instance.
- **Exceptions:**
  - `ArgumentException`: Thrown if the value is invalid (wrong length, format, or failed check digits).

### ToString()
- **Description:** Returns the CPF in formatted form.
- **Returns:** `string`

---

## Operators

### implicit operator string(Cpf cpf)
- **Description:** Allows implicit conversion from a `Cpf` object to a `string`.

### implicit operator Cpf(string value)
- **Description:** Allows implicit conversion from a `string` to a `Cpf` object, triggering validation.

---

## Usage Examples

```csharp
using LightningArc.Primitives.ValueObjects;

// Direct creation
var cpf = Cpf.Create("123.456.789-09");

// Implicit conversion
Cpf fromString = "123.456.789-09";

// Handling invalid input
if (Cpf.TryCreate("000.000.000-00", out var validCpf))
{
    // validCpf is guaranteed valid
}

// AsCpf for fluent handling
Result<Cpf> result = "invalid".AsCpf();

// Implicit conversion to Result<Cpf>
Result<Cpf> wrapped = cpf;
```