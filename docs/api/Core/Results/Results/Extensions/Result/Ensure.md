# ResultExtensions (Ensure)

**Namespace:** `LightningArc.Results`  
**Type:** `static partial class`

Provides methods to validate conditions within a result chain.

---

## Extension Methods

### Ensure(this Result result, bool condition, Error error)
Fails the result with the provided `error` if the `condition` is false.

### Ensure<TValue>(this Result<TValue> result, Func<TValue, bool> predicate, Error error)
Fails the result if the success value does not satisfy the `predicate`.

---

## Usage Example

```csharp
Result<int> ageResult = GetAge()
    .Ensure(age => age >= 18, Error.Validation.ValueOutOfRange("Must be an adult"));
```
