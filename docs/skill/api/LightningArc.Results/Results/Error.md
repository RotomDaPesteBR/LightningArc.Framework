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

## Error Categories (Modules)

The `Error` class is a partial class with several nested modules providing specific error types:

*   **Application**: Internal application errors.
*   **Authentication**: Security and identity errors.
*   **Validation**: Data integrity and format errors.
*   **Resource**: Resource existence (e.g., Not Found).
*   **Database**: Data persistence failures.
*   **Network/IO/System**: Infrastructure related errors.

---

## Usage Example

```csharp
// Using a predefined error module
Error error = Error.Validation.InvalidFormat("The provided date is invalid.");

// Adding details (implicitly or explicitly)
var validationError = Error.Validation.InvalidSchema("Validation failed", [
    new ErrorDetail("UserName", "Required"),
    ("Email", "Invalid format") // Implicit conversion from Tuple
]);

Console.WriteLine(validationError.Code); // e.g., 4002
foreach (var detail in validationError.Details)
{
    Console.WriteLine($"{detail.Context}: {detail.Message}");
}
```
