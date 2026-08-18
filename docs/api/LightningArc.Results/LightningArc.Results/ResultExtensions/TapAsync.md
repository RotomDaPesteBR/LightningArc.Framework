# ResultExtensions.TapAsync method (1 of 9)

Asynchronously executes the given action if the [`Result`](../Result.md) is a success.

```csharp
public static Task<Result> TapAsync(this Result result, Func<Task> action)
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
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.TapAsync method (2 of 9)

Asynchronously executes the given action if the Task is a success.

```csharp
public static Task<Result> TapAsync(this Task<Result> resultTask, Action action)
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
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.TapAsync method (3 of 9)

Asynchronously executes the given action if the Task is a success.

```csharp
public static Task<Result> TapAsync(this Task<Result> resultTask, Func<Task> action)
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
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.TapAsync&lt;T&gt; method (4 of 9)

Asynchronously executes the given action if the [`Result`](../Result-1.md) is a success.

```csharp
public static Task<Result<T>> TapAsync<T>(this Result<T> result, Func<T, Task> action)
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
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.TapAsync&lt;T&gt; method (5 of 9)

Asynchronously executes the given action if the [`Result`](../Result-1.md) is a success.

```csharp
public static Task<Result<T>> TapAsync<T>(this Result<T> result, Func<Task> action)
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
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.TapAsync&lt;T&gt; method (6 of 9)

Asynchronously executes the given action if the [`Result`](../Result-1.md) is a success.

```csharp
public static Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Action action)
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
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.TapAsync&lt;T&gt; method (7 of 9)

Asynchronously executes the given action if the [`Result`](../Result-1.md) is a success.

```csharp
public static Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Action<T> action)
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
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.TapAsync&lt;T&gt; method (8 of 9)

Asynchronously executes the given action if the [`Result`](../Result-1.md) is a success.

```csharp
public static Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Func<T, Task> action)
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
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.TapAsync&lt;T&gt; method (9 of 9)

Asynchronously executes the given action if the [`Result`](../Result-1.md) is a success.

```csharp
public static Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Func<Task> action)
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
* namespace [LightningArc.Results](../../LightningArc.Results.md)

<!-- DO NOT EDIT: generated by xmldocmd for LightningArc.Results.dll -->
