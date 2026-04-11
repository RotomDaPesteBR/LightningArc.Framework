# PhoneNumber

**Namespace:** `LightningArc.Primitives.ValueObjects`
**Type:** `record`

Represents a validated phone number. This type ensures immutability and validates the format against E.164 and common patterns.

---

## Properties

### Value
- **Type:** `string`
- **Description:** The phone number string value.

---

## Methods

### Create(string value)
- **Static:** Yes
- **Description:** Validates the input string against phone number format rules.
- **Parameters:**
  - `value` (`string`): The phone number string.
- **Returns:** A new `PhoneNumber` instance.
- **Exceptions:**
  - `ArgumentException`: Thrown if the value is null, empty, or does not match a valid phone number format.

### ToString()
- **Description:** Returns the phone number string value.
- **Returns:** `string`

---

## Operators

### implicit operator string(PhoneNumber phoneNumber)
- **Description:** Allows implicit conversion from a `PhoneNumber` object to a `string`.

### implicit operator PhoneNumber(string value)
- **Description:** Allows implicit conversion from a `string` to a `PhoneNumber` object.

---

## Usage Examples

```csharp
using LightningArc.Primitives.ValueObjects;

// Direct creation
var phone = PhoneNumber.Create("+5511999999999");

// Implicit conversion
PhoneNumber fromString = "+5511999999999";

// Handling invalid input
if (PhoneNumber.TryCreate("+5511999999999", out var validPhone))
{
    // validPhone is guaranteed valid
}

// AsPhoneNumber for fluent handling
Result<PhoneNumber> result = "invalid".AsPhoneNumber();

// Implicit conversion to Result<PhoneNumber>
Result<PhoneNumber> wrapped = phone;
```