# ResultExtensions (Match)

**Namespace:** `LightningArc.Results`  
**Type:** `static partial class`

Provides methods to "unwrap" or "match" a `Result`, forcing the handling of both success and failure cases.

---

## Extension Methods

### Match<TOut>(this Result result, Func<TOut> success, Func<Error, TOut> failure)
Converts a `Result` into a value of type `TOut`. This typically ends the chain.

### Match<TIn, TOut>(this Result<TIn> result, Func<Success<TIn>, TOut> success, Func<Error, TOut> failure)
Converts a `Result<TIn>` into a value of type `TOut`.

### Match<TOut>(this Result result, Func<Result<TOut>> success, Func<Error, Result<TOut>> failure)
Maps a result to a new `Result<TOut>` by explicitly handling both success and failure paths.

---

## Usage Example

```csharp
string display = result.Match(
    success => $"Success! Value: {success.Value}",
    error => $"Error {error.Code}: {error.Message}"
);
```
