# TaskResult<TValue>

**Namespace:** `LightningArc.Results`  
**Type:** `class`

A specialized wrapper for `Task<Result<TValue>>` that implements the Awaitable pattern. This allows you to `await` a `TaskResult` directly and use it in fluent chains without messy `Task` boilerplate.

---

## Static Methods (TaskResult)

*   **Success<TValue>(TValue value)**: Returns a `TaskResult` wrapping a successful result.
*   **Failure<TValue>(Error error)**: Returns a `TaskResult` wrapping a failed result.
*   **FromTask(Task<Result> task)**: Converts a standard `Task<Result>` into a `TaskResult<object>`.

---

## Usage Example

### Async Operations
```csharp
public TaskResult<User> GetUserAsync(int id)
{
    // Returns Task<Result<User>> which is implicitly converted
    return _service.FetchUser(id); 
}

// Awaiting directly
Result<User> result = await GetUserAsync(1);
```

### Fluent Chaining (Coming from Extensions)
`TaskResult` is designed to be used with extension methods like `MapAsync` and `BindAsync` to create clean asynchronous pipelines.
