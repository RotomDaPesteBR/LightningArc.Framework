# Error.Resource

**Namespace:** `LightningArc.Results`  
**Type:** `partial class` (Nested in `Error`)

Represents errors related to resource management (existence, state, availability).

---

## Constants

*   **CodePrefix**: 5

---

## Error Suffixes (Codes Enum)

*   **NotFound (1)**: The requested resource does not exist.
*   **AlreadyExists (2)**: Attempt to create a resource that already exists.
*   **Unavailable (3)**: The resource exists but cannot be accessed.
*   **InvalidState (4)**: The resource is not in the expected state.
*   **Obsolete (5)**: The requested resource is obsolete.

---

## Static Factory Methods

### NotFound(string? message = null, params IEnumerable<ErrorDetail>? details)
Creates an error with code `5001`.

### AlreadyExists(string? message = null, params IEnumerable<ErrorDetail>? details)
Creates an error with code `5002`.

### Unavailable(string? message = null, params IEnumerable<ErrorDetail>? details)
Creates an error with code `5003`.

### InvalidState(string? message = null, params IEnumerable<ErrorDetail>? details)
Creates an error with code `5004`.

### Obsolete(string? message = null, params IEnumerable<ErrorDetail>? details)
Creates an error with code `5005`.

---

## Usage Example

```csharp
return Error.Resource.NotFound("User with ID 123 was not found.");
```
