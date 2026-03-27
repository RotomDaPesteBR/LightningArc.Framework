# Result and Result<TValue>

**Namespace:** `LightningArc.Results`  
**Type:** `class`

The core component of the Result Pattern. It represents the outcome of an operation, which can either be a success or a failure.

---

## Result (Non-Generic)

Used for operations that do not return a specific value on success.

### Properties

*   **IsSuccess** (`bool`): Returns `true` if the operation was successful.
*   **IsFailure** (`bool`): Returns `true` if the operation failed.
*   **Code** (`int`): Returns the status code (from `SuccessDetails` if success, or `Error` if failure).
*   **Message** (`string?`): Returns the descriptive message associated with the result.
*   **Error** (`Error`): Gets the error object. Throws `ResultAccessFailedException` if accessed on a successful result.
*   **SuccessDetails** (`Success`): Gets the success metadata. Throws `ResultAccessFailedException` if accessed on a failed result.

### Static Factory Methods

*   **Success()**: Returns a standard success result (Ok).
*   **Success(Success success)**: Returns a success result with custom metadata.
*   **Created()**: Returns a success result with the "Created" code.
*   **Accepted()**: Returns a success result with the "Accepted" code.
*   **NoContent()**: Returns a success result with the "No Content" code.
*   **Failure(Error error)**: Returns a failure result encapsulating the provided error.

### Instance Methods

*   **WithValue<TValue>(TValue value)**: Converts the non-generic result into a `Result<TValue>`. If the original was a failure, the failure is propagated.

---

## Result<TValue> (Generic)

Inherits from `Result`. Used for operations that return a value of type `TValue` on success.

### Properties

*   **Value** (`TValue`): Gets the success value. Throws `ResultAccessFailedException` if the result is a failure.
*   **SuccessDetails** (`Success<TValue>`): Gets the typed success metadata.

### Static Factory Methods

*   **Success<TValue>(TValue value)**: Creates a success result with the provided value (Code: Ok).
*   **Success<TValue>(TValue value, string message)**: Success with value and custom message.
*   **Created<TValue>(TValue value)**: Created status with value.
*   **Accepted<TValue>(TValue value)**: Accepted status with value.
*   **NoContent<TValue>(TValue value)**: No Content status with value.
*   **Failure(Error error)**: Creates a failed `Result<TValue>`.

---

## Usage Examples

### Basic Flow
```csharp
public Result ProcessOrder(int orderId)
{
    if (orderId <= 0)
        return Error.Validation.ValueOutOfRange("Order ID must be positive");

    // Process logic...
    return Result.Success();
}
```

### Returning Values
```csharp
public Result<User> GetUser(int id)
{
    var user = _db.Users.Find(id);
    if (user == null)
        return Error.Resource.NotFound($"User {id} not found");

    return user; // Implicit conversion
}
```

### Handling Results
```csharp
var result = GetUser(1);
if (result.IsSuccess)
{
    Console.WriteLine($"Found: {result.Value.Name}");
}
else
{
    Console.WriteLine($"Error {result.Code}: {result.Message}");
}
```
