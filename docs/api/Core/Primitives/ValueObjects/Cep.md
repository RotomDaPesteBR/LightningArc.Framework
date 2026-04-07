# Cep

**Namespace:** `LightningArc.Primitives.ValueObjects`
**Type:** `record`

Represents a validated Brazilian CEP (Cadastro de Enderecos Postais) number. This type ensures immutability and normalizes the input to the `XXXXX-XXX` format.

---

## Properties

### Value
- **Type:** `string`
- **Description:** The CEP number formatted as `XXXXX-XXX`.

---

## Methods

### Create(string value)
- **Static:** Yes
- **Description:** Validates the input string against the CEP format and normalizes it.
- **Parameters:**
  - `value` (`string`): The raw or formatted CEP string (accepts 8 digits with or without hyphen).
- **Returns:** A new `Cep` instance.
- **Exceptions:**
  - `ArgumentException`: Thrown if the value is invalid (wrong format or length).

### TryCreate(string value, out Cep? result)
- **Static:** Yes
- **Description:** Attempts to create a `Cep` without throwing exceptions.
- **Parameters:**
  - `value` (`string`): The raw CEP string.
  - `result` (`out Cep?`): The created instance if valid, or `null`.
- **Returns:** `bool` — `true` if the CEP is valid; otherwise, `false`.

### ToString()
- **Description:** Returns the CEP in `XXXXX-XXX` format.
- **Returns:** `string`

---

## Operators

### implicit operator string(Cep cep)
- **Description:** Allows implicit conversion from a `Cep` object to a `string`.

### implicit operator Cep(string value)
- **Description:** Allows implicit conversion from a `string` to a `Cep` object, triggering validation.

---

## Usage Examples

```csharp
using LightningArc.Primitives.ValueObjects;

// Direct creation (normalizes automatically)
var cep = Cep.Create("12345678");      // "12345-678"
var cep2 = Cep.Create("12345-678");    // "12345-678"

// TryCreate for safe handling
if (Cep.TryCreate("01001-000", out var validCep))
{
    // validCep is guaranteed valid
}

// ToResult for fluent handling
Result<Cep> result = "invalid".CreateCepResult();
```
