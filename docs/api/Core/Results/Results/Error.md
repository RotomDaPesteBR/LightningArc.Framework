# Error

**Namespace:** `LightningArc.Results`  
**Type:** `partial class`

Represents a standardized error in the application. Errors are categorized by a prefix (Module) and a suffix (Specific Error).

---

## Properties

*   **Code** (`int`): The full numeric error code (Prefix * 1000 + Suffix).
*   **Message** (`string`): The descriptive, potentially localized, error message.
*   **Details** (`IReadOnlyList<ErrorDetail>`): A collection of granular error details (e.g., validation failures for specific fields).

---

## Operators & Methods

### Deconstruction
Allows positional extraction of error properties.
- `(int code, string message)`
- `(int code, string message, IReadOnlyList<ErrorDetail> details)`

### Operator `+`
Combines two errors into an [AggregateError](Errors/AggregateError.md).
- If codes are identical, the result maintains the code.
- If codes differ, a General Aggregate code (99001) is used.

### Equality (`==`, `!=`)
Compares errors based on Code and Details (ignores localized messages).

---

## Error Categories (Modules)

The `Error` class is a partial class with several nested modules providing specific error types:

*   **Application**: Internal application errors.
*   **Authentication**: Security and identity errors.
*   **Validation**: Data integrity and format errors.
*   **Resource**: Resource existence (e.g., Not Found).
*   **Database**: Data persistence failures.
*   **Network/IO/System**: Infrastructure related errors.
*   **General**: Module 99, used for aggregate errors.

---

## Usage Example

```csharp
// Using a predefined error module
Error error = Error.Validation.InvalidFormat("The provided date is invalid.");

// Combining errors
Error combined = error + Error.Validation.MissingField("Missing required ID");

// Deconstructing
var (code, msg) = combined;
```
