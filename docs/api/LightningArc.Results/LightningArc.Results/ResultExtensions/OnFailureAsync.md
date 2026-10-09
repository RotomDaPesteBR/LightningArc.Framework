# ResultExtensions.OnFailureAsync method (1 of 12)

Asynchronously executes the given action if the [`Result`](../Result.md) is a failure.

```csharp
public static Task<Result> OnFailureAsync(this Result result, Func<Error, Task> action)
```

| parameter | description |
| --- | --- |
| result | The input [`Result`](../Result.md). |
| action | The asynchronous action to execute. |

## Return Value

The input [`Result`](../Result.md).

## See Also

* class [Result](../Result.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.OnFailureAsync method (2 of 12)

Asynchronously executes the given action if the [`Result`](../Result.md) is a failure.

```csharp
public static Task<Result> OnFailureAsync(this Result result, Func<Task> action)
```

| parameter | description |
| --- | --- |
| result | The input [`Result`](../Result.md). |
| action | The asynchronous action to execute. |

## Return Value

The input [`Result`](../Result.md).

## See Also

* class [Result](../Result.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.OnFailureAsync method (3 of 12)

Asynchronously executes the given action if the Task is a failure.

```csharp
public static Task<Result> OnFailureAsync(this Task<Result> resultTask, Action action)
```

| parameter | description |
| --- | --- |
| resultTask | The input Task. |
| action | The action to execute. |

## Return Value

The input Task.

## See Also

* class [Result](../Result.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.OnFailureAsync method (4 of 12)

Asynchronously executes the given action if the Task is a failure.

```csharp
public static Task<Result> OnFailureAsync(this Task<Result> resultTask, Action<Error> action)
```

| parameter | description |
| --- | --- |
| resultTask | The input Task. |
| action | The action to execute. |

## Return Value

The input Task.

## See Also

* class [Result](../Result.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.OnFailureAsync method (5 of 12)

Asynchronously executes the given action if the Task is a failure.

```csharp
public static Task<Result> OnFailureAsync(this Task<Result> resultTask, Func<Error, Task> action)
```

| parameter | description |
| --- | --- |
| resultTask | The input Task. |
| action | The asynchronous action to execute. |

## Return Value

The input Task.

## See Also

* class [Result](../Result.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.OnFailureAsync method (6 of 12)

Asynchronously executes the given action if the Task is a failure.

```csharp
public static Task<Result> OnFailureAsync(this Task<Result> resultTask, Func<Task> action)
```

| parameter | description |
| --- | --- |
| resultTask | The input Task. |
| action | The asynchronous action to execute. |

## Return Value

The input Task.

## See Also

* class [Result](../Result.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.OnFailureAsync&lt;T&gt; method (7 of 12)

Asynchronously executes the given action if the [`Result`](../Result-1.md) is a failure.

```csharp
public static Task<Result<T>> OnFailureAsync<T>(this Result<T> result, Func<Error, Task> action)
```

| parameter | description |
| --- | --- |
| T | The type of the value. |
| result | The input [`Result`](../Result-1.md). |
| action | The asynchronous action to execute. |

## Return Value

The input [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.OnFailureAsync&lt;T&gt; method (8 of 12)

Asynchronously executes the given action if the [`Result`](../Result-1.md) is a failure.

```csharp
public static Task<Result<T>> OnFailureAsync<T>(this Result<T> result, Func<Task> action)
```

| parameter | description |
| --- | --- |
| T | The type of the value. |
| result | The input [`Result`](../Result-1.md). |
| action | The asynchronous action to execute. |

## Return Value

The input [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.OnFailureAsync&lt;T&gt; method (9 of 12)

Asynchronously executes the given action if the [`Result`](../Result-1.md) is a failure.

```csharp
public static Task<Result<T>> OnFailureAsync<T>(this Task<Result<T>> resultTask, Action action)
```

| parameter | description |
| --- | --- |
| T | The type of the value. |
| resultTask | The input [`Result`](../Result-1.md). |
| action | The action to execute. |

## Return Value

The input [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.OnFailureAsync&lt;T&gt; method (10 of 12)

Asynchronously executes the given action if the [`Result`](../Result-1.md) is a failure.

```csharp
public static Task<Result<T>> OnFailureAsync<T>(this Task<Result<T>> resultTask, 
    Action<Error> action)
```

| parameter | description |
| --- | --- |
| T | The type of the value. |
| resultTask | The input [`Result`](../Result-1.md). |
| action | The action to execute. |

## Return Value

The input [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.OnFailureAsync&lt;T&gt; method (11 of 12)

Asynchronously executes the given action if the [`Result`](../Result-1.md) is a failure.

```csharp
public static Task<Result<T>> OnFailureAsync<T>(this Task<Result<T>> resultTask, 
    Func<Error, Task> action)
```

| parameter | description |
| --- | --- |
| T | The type of the value. |
| resultTask | The input [`Result`](../Result-1.md). |
| action | The asynchronous action to execute. |

## Return Value

The input [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.OnFailureAsync&lt;T&gt; method (12 of 12)

Asynchronously executes the given action if the [`Result`](../Result-1.md) is a failure.

```csharp
public static Task<Result<T>> OnFailureAsync<T>(this Task<Result<T>> resultTask, Func<Task> action)
```

| parameter | description |
| --- | --- |
| T | The type of the value. |
| resultTask | The input [`Result`](../Result-1.md). |
| action | The asynchronous action to execute. |

## Return Value

The input [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

<!-- DO NOT EDIT: generated by xmldocmd for LightningArc.Results.dll -->
