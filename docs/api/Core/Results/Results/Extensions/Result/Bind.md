# ResultExtensions (Bind)

**Namespace:** `LightningArc.Results`  
**Type:** `static partial class`

This part of the `ResultExtensions` class provides methods for chaining operations that return a `Result`.

---

## Extension Methods

### Bind<TOut>(this Result result, Func<Result<TOut>> mapper)
Chains a non-generic result to a function that returns a `Result<TOut>`.

### Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> mapper)
Chains a generic result to a function that takes the success value and returns a new `Result<TOut>`.

---

## Usage Example

```csharp
// Chaining operations that can fail
Result<User> result = GetUserId("username")
    .Bind(id => Database.FetchUser(id));
```
