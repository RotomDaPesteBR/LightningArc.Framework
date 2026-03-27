# Success.cs Implementation Details

**File Path:** `Core/Results/Results/Success.cs`
**Namespace:** `LightningArc.Results`

## Overview
The `Success` class and its generic counterpart `Success<TValue>` represent the metadata of a successful operation. While a simple boolean `true` is enough to say "it worked", `Success` allows conveying *how* it worked (e.g., "Created", "Accepted", "No Content") and carrying an optional message.

## Code Analysis

### Base Class: `Success`
*   **Purpose**: Holds metadata (Code, Message) for operations that don't return a value.
*   **Codes**:
    *   100: OK
    *   101: Created
    *   102: Accepted
    *   103: No Content
*   **Factory Methods**: Static methods like `Ok()`, `Created()`, etc., create instances of internal sealed classes (`OkSuccess`, `CreatedSuccess`) that inherit from `Success`.

### Generic Class: `Success<TValue>`
*   **Purpose**: Extends `Success` to hold a value (`TValue`).
*   **Design**: Inherits from `Success`.
*   **Internal Subclasses**: Similar to the base class, it uses internal sealed subclasses (`OkSuccess`, `CreatedSuccess`, etc.) to represent specific success types while carrying the value.

### Polymorphism
The `WithValue<TValue>(TValue value)` abstract method allows transforming a non-generic success into a generic one while preserving the original status code and message type.
