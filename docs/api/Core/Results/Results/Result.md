# Result and Result<TValue>

**Namespace:** `LightningArc.Results`  
**Type:** `class`

The core component of the Result Pattern. It represents the outcome of an operation, which can either be a success or a failure.

---

## Syntactic Sugar & Operators

Both `Result` and `Result<TValue>` support the following idiomatic C# features:

### Boolean Operators
Results can be used directly in conditional statements:
- `if (result)`: Executes if `IsSuccess` is true.
- `if (!result)`: Executes if `IsFailure` is true.

### Deconstruction
Extract properties into local variables:
- `var (isSuccess, error) = result;`
- `var (isSuccess, value, error) = genericResult;`

### Explicit Casting (Generic Only)
Direct access via casting:
- `TValue val = (TValue)result;`
- `Error err = (Error)result;`

---

## Result (Non-Generic)

Used for operations that do not return a specific value on success.

### Properties

*   **IsSuccess** (`bool`): Returns `true` if the operation was successful.
*   **IsFailure** (`bool`): Returns `true` if the operation failed.
*   **Code** (`int`): Returns the status code (from `SuccessDetails` if success, or `Error` if failure).
*   **Message** (`string?`): Returns the descriptive message associated with the result.
*   **Error** (`Error`): Gets the error object. Throws `ResultAccessFailedException` if accessed on a successful result.
*   **SuccessDetails** (`Success`): Gets the success metadata.

---

## Result<TValue> (Generic)

Inherits from `Result`. Used for operations that return a value of type `TValue` on success.

### Properties

*   **Value** (`TValue`): Gets the success value. Throws `ResultAccessFailedException` if the result is a failure.
*   **SuccessDetails** (`Success<TValue>`): Gets the typed success metadata.

---

## Usage Examples

### Modern Usage
```csharp
var result = GetUser(1);

if (result) // Boolean operator
{
    var (success, user, _) = result; // Deconstruction
    Console.WriteLine($"Found: {user.Name}");
}
else
{
    Console.WriteLine($"Error: {result.Error.Message}");
}

// Explicit cast
User u = (User)GetUser(1);
```
