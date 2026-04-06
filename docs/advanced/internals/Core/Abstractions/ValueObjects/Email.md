# Email.cs Implementation Details

**File Path:** `Core/Abstractions/ValueObjects/Email.cs`
**Namespace:** `LightningArc.Primitives.ValueObjects`

## Overview
The `Email` class is a **Value Object** implemented as a C# `record`. It encapsulates the concept of an email address, ensuring that any instance of this type contains a syntactically valid email string. It enforces validation at the time of creation, preventing the existence of invalid `Email` objects within the domain.

## Code Analysis

### Class Definition
```csharp
public record Email
```
*   **`record`**: Defines the type as an immutable reference type with built-in value-based equality semantics. This is ideal for Value Objects, where identity is defined by the value itself (the email string) rather than a memory reference.

### Regex Pattern
```csharp
private const string EmailRegexPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
```
*   **Purpose**: A Regular Expression used to validate the format of the email string.
*   **Logic**:
    *   `^[^@\s]+`: Starts with one or more characters that are *not* `@` or whitespace.
    *   `@`: Followed by exactly one `@` symbol.
    *   `[^@\s]+`: Followed by the domain part (characters not `@` or whitespace).
    *   `\.`: A literal dot `.` separator.
    *   `[^@\s]+$`: Ends with the top-level domain (characters not `@` or whitespace).
*   **Robustness**: This is a simplified but practical regex that catches most common formatting errors without being overly restrictive (compliant with most practical use cases, if not fully RFC 5322).

### Properties
```csharp
public string Value { get; }
```
*   **Immutability**: The `Value` property holds the actual email string. It is read-only (`get;`), preserving the immutability of the record.

### Constructor (Private)
```csharp
private Email(string value)
```
*   **Privacy**: The constructor is `private` to force the use of the static Factory Method (`Create`). This ensures validation logic cannot be bypassed.
*   **Validation Logic**:
    1.  **Null/Empty Check**: Throws `ArgumentException` if the input is null or whitespace.
    2.  **Format Check**: Uses `Regex.IsMatch` with a timeout (250ms) to prevent Regex Denial of Service (ReDoS) attacks. Throws `ArgumentException` if the format is invalid.

### Factory Method
```csharp
public static Email Create(string value) => new Email(value);
```
*   **Pattern**: Static Factory Method.
*   **Purpose**: Provides the primary way to instantiate an `Email` object. It delegates to the private constructor where validation occurs.

### Implicit Operators
The class implements `implicit` operators to simplify usage and make the type feel more native.

1.  **`Email` to `string`**:
    ```csharp
    public static implicit operator string(Email email) => email.Value;
    ```
    Allows an `Email` object to be assigned directly to a `string` variable or used in string interpolation.

2.  **`string` to `Email`**:
    ```csharp
    public static implicit operator Email(string value) => Create(value);
    ```
    Allows a `string` to be assigned directly to an `Email` variable. Note that this **will throw an exception** if the string is invalid, so it should be used with caution.

### Overrides
```csharp
public override string ToString() => Value;
```
*   **Purpose**: Returns the raw email string when `ToString()` is called, making debugging and logging cleaner.
