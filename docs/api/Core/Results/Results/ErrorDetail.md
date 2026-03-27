# ErrorDetail

**Namespace:** `LightningArc.Results`  
**Type:** `readonly struct`

Provides granular details for an `Error`. This is typically used for field-level validation messages.

---

## Properties

*   **Context** (`string`): An identifier for the error source (e.g., "Email", "Password", "UserId").
*   **Message** (`string`): The specific error message for this context.

---

## Operators

### implicit operator ErrorDetail((string Context, string Message) value)
Allows creating an `ErrorDetail` from a simple tuple.

---

## Usage Example

```csharp
// Manual creation
var detail = new ErrorDetail("Age", "Value must be between 18 and 99");

// Implicit from Tuple
ErrorDetail implicitDetail = ("Price", "Cannot be negative");
```
