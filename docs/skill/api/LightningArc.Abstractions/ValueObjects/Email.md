# Email

**Namespace:** `LightningArc.Abstractions.ValueObjects`  
**Type:** `record`

Represents a validated email address value object. This type ensures immutability and guarantees that the email format is valid at the moment of creation.

---

## Properties

### Value
*   **Type:** `string`
*   **Description:** Gets the string value of the email address.

---

## Methods

### Create(string value)
*   **Static:** Yes
*   **Description:** The preferred public method to construct an `Email` object. It validates the input string against a standard email regex.
*   **Parameters:**
    *   `value` (`string`): The string representing the email address.
*   **Returns:** A new `Email` instance.
*   **Exceptions:** 
    *   `ArgumentException`: Thrown if the value is null, empty, or does not match the email format.

### ToString()
*   **Description:** Returns the email address string.
*   **Returns:** `string`

---

## Operators

### implicit operator string(Email email)
*   **Description:** Allows implicit conversion from an `Email` object to a `string`.

### implicit operator Email(string value)
*   **Description:** Allows implicit conversion from a `string` to an `Email` object. This automatically triggers the validation logic.

---

## Usage Example

```csharp
using LightningArc.Abstractions.ValueObjects;

// Direct creation
var email = Email.Create("contact@lightningarc.com");

// Implicit conversion to string
string raw = email; 

// Implicit conversion from string (triggers validation)
Email fromString = "test@example.com";

// Handling invalid input
try 
{
    Email invalid = "not-an-email";
}
catch (ArgumentException ex)
{
    // Output: The value 'not-an-email' is not a valid email address.
    Console.WriteLine(ex.Message);
}
```
