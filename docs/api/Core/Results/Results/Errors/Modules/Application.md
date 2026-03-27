# Error.Application

**Namespace:** `LightningArc.Results`  
**Type:** `partial class` (Nested in `Error`)

Represents high-level and generic application errors.

---

## Constants

*   **CodePrefix**: 1

---

## Error Suffixes (Codes Enum)

*   **Internal (1)**: Generic error for any unmapped internal failure.
*   **InvalidParameter (2)**: One or more parameters in an internal function call are invalid.
*   **InvalidOperation (3)**: An operation is logically invalid in the current state.
*   **TaskCanceled (4)**: An operation was prematurely interrupted or canceled.
*   **NotImplemented (5)**: Resource, method, or functionality not yet implemented.

---

## Static Factory Methods

### Internal(string? message = null, params IEnumerable<ErrorDetail>? details)
Creates an error with code `1001`.

### InvalidParameter(string? message = null, params IEnumerable<ErrorDetail>? details)
Creates an error with code `1002`.

### InvalidOperation(string? message = null, params IEnumerable<ErrorDetail>? details)
Creates an error with code `1003`.

### TaskCanceled(string? message = null, params IEnumerable<ErrorDetail>? details)
Creates an error with code `1004`.

### NotImplemented(string? message = null, params IEnumerable<ErrorDetail>? details)
Creates an error with code `1005`.

---

## Usage Example

```csharp
return Error.Application.Internal("An unexpected error occurred.");
```
