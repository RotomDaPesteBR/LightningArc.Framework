# Success and Success<TValue>

**Namespace:** `LightningArc.Results`  
**Type:** `abstract class`

Standardized success metadata objects. These classes define specific success categories (Ok, Created, Accepted, NoContent) and can encapsulate return values.

---

## Success (Non-Generic)

### Properties

*   **Code** (`int`): The numeric success code.
*   **Message** (`string?`): The localized success message.

### Static Factory Methods

*   **Ok(string? message = null)**: Code 100.
*   **Created(string? message = null)**: Code 101.
*   **Accepted(string? message = null)**: Code 102.
*   **NoContent(string? message = null)**: Code 103.

### Instance Methods

*   **WithValue<TValue>(TValue value)**: Transforms the metadata into a typed `Success<TValue>`.

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

// Creating a typed success from existing metadata
Success<int> typedMetadata = metadata.WithValue(42);

Console.WriteLine(typedMetadata.Code);    // 101
Console.WriteLine(typedMetadata.Value);   // 42
```
