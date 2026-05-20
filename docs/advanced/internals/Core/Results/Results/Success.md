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
*   **Factory Methods**: Static methods like `Ok()`, `Created()`, etc., create instances of concrete sealed types from the `LightningArc.Results.Successes` namespace.

### Concrete Success Types
The concrete success types are defined in individual files under `Core/Results/Results/Successes/` in the `LightningArc.Results.Successes` namespace:

| File | Non-Generic Type | Generic Type | Code |
| :--- | :--- | :--- | :--- |
| `OkSuccess.cs` | `OkSuccess` | `OkSuccess<TValue>` | 100 |
| `CreatedSuccess.cs` | `CreatedSuccess` | `CreatedSuccess<TValue>` | 101 |
| `AcceptedSuccess.cs` | `AcceptedSuccess` | `AcceptedSuccess<TValue>` | 102 |
| `NoContentSuccess.cs` | `NoContentSuccess` | `NoContentSuccess<TValue>` | 103 |

### Generic Class: `Success<TValue>`
*   **Purpose**: Extends `Success` to hold a value (`TValue`).
*   **Design**: Inherits from `Success`.
*   **Generic Subclasses**: Each non-generic success type has a generic counterpart (e.g., `OkSuccess<TValue>`) that carries the typed value while preserving the status code and message type.

### Polymorphism
The `WithValue<TValue>(TValue value)` abstract method allows transforming a non-generic `Success` into a `Success<TValue>` while preserving the original status code and message type.

### Hook
`Success.Hook` is a fluent marker type used to attach domain-specific result extensions via extension methods (accessed via `Result.Of`). It remains nested in `Success` for ergonomic access.