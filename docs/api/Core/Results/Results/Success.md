# Success and Success<TValue>

**Namespace:** `LightningArc.Results`  
**Type:** `abstract class`

Standardized success metadata objects. These classes define specific success categories and can encapsulate return values.

---

## Success Subclasses

The following subclasses are **public sealed** and can be used for type checking:

*   **OkSuccess**: Representing code 100 (Ok).
*   **CreatedSuccess**: Representing code 101 (Created).
*   **AcceptedSuccess**: Representing code 102 (Accepted).
*   **NoContentSuccess**: Representing code 103 (No Content).

---

## Success (Non-Generic)

### Properties

*   **Code** (`int`): The numeric success code.
*   **Message** (`string?`): The localized success message.

### Static Factory Methods

*   **Ok(string? message = null)**: Returns `OkSuccess`.
*   **Created(string? message = null)**: Returns `CreatedSuccess`.
*   **Accepted(string? message = null)**: Returns `AcceptedSuccess`.
*   **NoContent(string? message = null)**: Returns `NoContentSuccess`.

---

## Success<TValue> (Generic)

Inherits from `Success`. Encapsulates a value.

### Properties

*   **Value** (`TValue`): The result value.

---

## Usage Example

```csharp
// Using factory methods
Success metadata = Success.Created("Resource created successfully");

// Type checking
if (metadata is Success.CreatedSuccess) { ... }

// Creating a typed success
Success<int> typedMetadata = metadata.WithValue(42);
```
