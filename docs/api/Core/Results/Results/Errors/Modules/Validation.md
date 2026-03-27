# Error.Validation

**Namespace:** `LightningArc.Results`  
**Type:** `partial class` (Nested in `Error`)

Represents errors related to data input and business rule validation.

---

## Constants

*   **CodePrefix**: 4

---

## Error Suffixes (Codes Enum)

*   **InvalidFormat (1)**: Data format (JSON, XML, etc.) is incorrect.
*   **InvalidSchema (2)**: Data structure does not match expected schema.
*   **DeserializationFailed (3)**: Failed to convert string/bytes to object.
*   **MissingField (4)**: A required data field is missing.
*   **ValueOutOfRange (5)**: A field value is outside allowed bounds.

---

## Static Factory Methods

### InvalidFormat(string? message = null, params IEnumerable<ErrorDetail>? details)
Creates an error with code `4001`.

### InvalidSchema(string? message = null, params IEnumerable<ErrorDetail>? details)
Creates an error with code `4002`.

### DeserializationFailed(string? message = null, params IEnumerable<ErrorDetail>? details)
Creates an error with code `4003`.

### MissingField(string? message = null, params IEnumerable<ErrorDetail>? details)
Creates an error with code `4004`.

### ValueOutOfRange(string? message = null, params IEnumerable<ErrorDetail>? details)
Creates an error with code `4005`.

---

## Usage Example

```csharp
var details = new List<ErrorDetail> { ("Age", "Must be > 0") };
return Error.Validation.ValueOutOfRange("Validation failed", details);
```
