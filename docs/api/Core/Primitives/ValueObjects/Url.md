# Url

**Namespace:** `LightningArc.Primitives.ValueObjects`
**Type:** `record`

Represents a validated URL. This type ensures immutability and validates the format against standard URL patterns.

---

## Properties

### Value
- **Type:** `string`
- **Description:** The URL string value.

---

## Methods

### Create(string value)
- **Static:** Yes
- **Description:** Validates the input string against URL format rules.
- **Parameters:**
  - `value` (`string`): The URL string.
- **Returns:** A new `Url` instance.
- **Exceptions:**
  - `ArgumentException`: Thrown if the value is null, empty, or does not match a valid URL format.

### ToString()
- **Description:** Returns the URL string value.
- **Returns:** `string`

---

## Operators

### implicit operator string(Url url)
- **Description:** Allows implicit conversion from a `Url` object to a `string`.

### implicit operator Url(string value)
- **Description:** Allows implicit conversion from a `string` to a `Url` object.

---

## Usage Examples

```csharp
using LightningArc.Primitives.ValueObjects;

// Direct creation
var url = Url.Create("https://example.com/path");

// Implicit conversion
Url fromString = "https://example.com/path";

// Handling invalid input
if (Url.TryCreate("https://example.com", out var validUrl))
{
    // validUrl is guaranteed valid
}

// AsUrl for fluent handling
Result<Url> result = "invalid-url".AsUrl();

// Implicit conversion to Result<Url>
Result<Url> wrapped = url;
```