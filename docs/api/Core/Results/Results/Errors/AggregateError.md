# AggregateError

**Namespace:** `LightningArc.Results`  
**Type:** `sealed class`  
**Inherits from:** [Error](../Error.md)

Represents a group of one or more errors that occurred during an operation. This class is useful for aggregating multiple errors, such as those from multiple validation rules, into a single error object.

---

## Properties

*   **Errors** (`ReadOnlyCollection<Error>`): Gets the collection of the [Error](../Error.md) instances that caused the current error.
*   **Code** (`int`): Inherited. Returns the aggregate code (99001 if mixed categories).
*   **Details** (`IReadOnlyList<ErrorDetail>`): Inherited. Contains the union of all details from the aggregated errors.

---

## Methods

### Flatten()
Flattens the `AggregateError` instances into a single list of non-aggregate errors.
- **Returns**: A new `AggregateError` containing only non-aggregate errors.

---

## Usage Example

```csharp
Error e1 = Error.Validation.MissingField("Name is required");
Error e2 = Error.Validation.InvalidFormat("Email is invalid");

// Create aggregate using operator
Error aggregate = e1 + e2;

if (aggregate is AggregateError agg)
{
    Console.WriteLine($"Total errors: {agg.Errors.Count}");
    
    // Flatten nested aggregates
    var simpleList = agg.Flatten().Errors;
}
```
