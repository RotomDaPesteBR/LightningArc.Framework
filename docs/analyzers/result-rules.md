# Result Rules

Rules LARC001–LARC003 enforce safe usage of the Result pattern.

---

## LARC001

**Unsafe `.Value` access without `IsSuccess` check**

Accessing `Result<T>.Value` when the result is a failure throws `ResultAccessFailedException`.

### ❌ Bad

```csharp
int val = result.Value; // throws if IsFailure
```

### ✅ Good

```csharp
if (result.TryGetValue(out int val))
{
    // use val
}
```

A code fix is available that automatically wraps the unsafe access in a `TryGetValue` check.

---

## LARC002

**Unsafe `.Error` access without `IsFailure` check**

Accessing `Result.Error` when the result is a success throws `ResultAccessFailedException`.

### ❌ Bad

```csharp
var err = result.Error; // throws if IsSuccess
```

### ✅ Good

```csharp
if (result.TryGetError(out var err))
{
    // handle err
}
```

A code fix is available that automatically wraps the unsafe access in a `TryGetError` check.

---

## LARC003

**Result return value discarded**

Calling a method that returns `Result` or `Result<T>` without using the return value silently ignores potential failures.

### ❌ Bad

```csharp
DoWork();  // Result returned but ignored
```

### ✅ Good

```csharp
var result = DoWork();
if (result.IsFailure) { /* handle */ }

// Or discard explicitly with _
_ = DoWork();  // intentional
```
