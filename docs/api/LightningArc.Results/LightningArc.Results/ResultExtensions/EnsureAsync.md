# ResultExtensions.EnsureAsync method (1 of 6)

Asynchronously ensures that the given condition is true, otherwise returns a new failure [`Result`](../Result.md).

```csharp
public static Task<Result> EnsureAsync(this Result result, Func<Task<bool>> condition, Error error)
```

| parameter | description |
| --- | --- |
| result | The input [`Result`](../Result.md). |
| condition | The asynchronous condition to check. |
| error | The error to return if the condition is false. |

## Return Value

The input [`Result`](../Result.md) if the condition is true, otherwise a new failure [`Result`](../Result.md).

## See Also

* class [Result](../Result.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.EnsureAsync method (2 of 6)

Asynchronously ensures that the given condition is true, otherwise returns a new failure [`Result`](../Result.md).

```csharp
public static Task<Result> EnsureAsync(this Task<Result> resultTask, bool condition, Error error)
```

| parameter | description |
| --- | --- |
| resultTask | The input Task. |
| condition | The condition to check. |
| error | The error to return if the condition is false. |

## Return Value

The input [`Result`](../Result.md) if the condition is true, otherwise a new failure [`Result`](../Result.md).

## See Also

* class [Result](../Result.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.EnsureAsync method (3 of 6)

Asynchronously ensures that the given condition is true, otherwise returns a new failure [`Result`](../Result.md).

```csharp
public static Task<Result> EnsureAsync(this Task<Result> resultTask, Func<Task<bool>> condition, 
    Error error)
```

| parameter | description |
| --- | --- |
| resultTask | The input Task. |
| condition | The asynchronous condition to check. |
| error | The error to return if the condition is false. |

## Return Value

The input [`Result`](../Result.md) if the condition is true, otherwise a new failure [`Result`](../Result.md).

## See Also

* class [Result](../Result.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.EnsureAsync&lt;T&gt; method (4 of 6)

Asynchronously ensures that the given predicate is true, otherwise returns a new failure [`Result`](../Result.md).

```csharp
public static Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<bool>> predicate, 
    Error error)
```

| parameter | description |
| --- | --- |
| T | The type of the value. |
| result | The input [`Result`](../Result-1.md). |
| predicate | The asynchronous predicate to check. |
| error | The error to return if the predicate is false. |

## Return Value

The input [`Result`](../Result-1.md) if the predicate is true, otherwise a new failure [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.EnsureAsync&lt;T&gt; method (5 of 6)

Asynchronously ensures that the given predicate is true, otherwise returns a new failure [`Result`](../Result.md).

```csharp
public static Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, 
    Func<T, bool> predicate, Error error)
```

| parameter | description |
| --- | --- |
| T | The type of the value. |
| resultTask | The input [`Result`](../Result-1.md). |
| predicate | The predicate to check. |
| error | The error to return if the predicate is false. |

## Return Value

The input [`Result`](../Result-1.md) if the predicate is true, otherwise a new failure [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.EnsureAsync&lt;T&gt; method (6 of 6)

Asynchronously ensures that the given predicate is true, otherwise returns a new failure [`Result`](../Result.md).

```csharp
public static Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, 
    Func<T, Task<bool>> predicate, Error error)
```

| parameter | description |
| --- | --- |
| T | The type of the value. |
| resultTask | The input [`Result`](../Result-1.md). |
| predicate | The asynchronous predicate to check. |
| error | The error to return if the predicate is false. |

## Return Value

The input [`Result`](../Result-1.md) if the predicate is true, otherwise a new failure [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

<!-- DO NOT EDIT: generated by xmldocmd for LightningArc.Results.dll -->
