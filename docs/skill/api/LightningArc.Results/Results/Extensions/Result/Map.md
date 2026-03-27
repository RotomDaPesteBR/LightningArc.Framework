# ResultExtensions (Map)

**Namespace:** `LightningArc.Results`  
**Type:** `static partial class`

This part of the `ResultExtensions` class provides methods for transforming successful results.

---

## Extension Methods

### Map<TOut>(this Result result, Func<TOut> mapper)
Transforms a non-generic `Result` into a `Result<TOut>`.
*   **If Success:** Executes the mapper and returns `Result.Success(value)`.
*   **If Failure:** Returns the original error.

### Map<TOut>(this Result result, Func<Success<TOut>> mapper)
Transforms a `Result` into a `Result<TOut>` using specific success metadata.

### Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper)
Transforms a `Result<TIn>` into a `Result<TOut>` by applying a function to its value.

### Map<TIn, TOut>(this Result<TIn> result, Func<TIn, Success<TOut>> mapper)
Transforms a `Result<TIn>` into a `Result<TOut>` by applying a function that returns `Success<TOut>`.

---

## Usage Example

```csharp
Result<int> stringLength = Result.Success("Hello")
    .Map(str => str.Length);

Console.WriteLine(stringLength.Value); // 5
```
