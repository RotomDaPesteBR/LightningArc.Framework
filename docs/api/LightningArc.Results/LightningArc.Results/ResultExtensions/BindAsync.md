# ResultExtensions.BindAsync&lt;TOut&gt; method (1 of 6)

Asynchronously binds a non-generic [`Result`](../Result.md) to a new [`Result`](../Result-1.md) by applying an asynchronous function. This method is used to chain asynchronous operations where the next step (defined by the *mapper*) also returns a [`Result`](../Result-1.md), allowing for propagation of success or failure.

```csharp
public static Task<Result<TOut>> BindAsync<TOut>(this Result result, 
    Func<Task<Result<TOut>>> mapper)
```

| parameter | description |
| --- | --- |
| TOut | The type of the value contained in the output [`Result`](../Result-1.md). |
| result | The input non-generic [`Result`](../Result.md). |
| mapper | The asynchronous function to apply if the input *result* is successful. This function takes no arguments and returns a [`Result`](../Result-1.md). |

## Return Value

A [`Result`](../Result-1.md) representing the asynchronous operation: If the input *result* is successful, the result of applying the *mapper* function. If the input *result* is a failure, a [`Result`](../Result-1.md) containing the original error is returned.

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Result](../Result.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.BindAsync&lt;TOut&gt; method (2 of 6)

Asynchronously binds a Task to a new [`Result`](../Result-1.md) by applying a synchronous function. This method awaits the input Task and then applies a synchronous binding function if the awaited result is successful, allowing for propagation of success or failure.

```csharp
public static Task<Result<TOut>> BindAsync<TOut>(this Task<Result> resultTask, 
    Func<Result<TOut>> mapper)
```

| parameter | description |
| --- | --- |
| TOut | The type of the value contained in the output [`Result`](../Result-1.md). |
| resultTask | The input Task representing an asynchronous operation that yields a non-generic result. |
| mapper | The synchronous function to apply if the awaited *resultTask* is successful. This function takes no arguments and returns a [`Result`](../Result-1.md). |

## Return Value

A [`Result`](../Result-1.md) representing the asynchronous operation: If the awaited *resultTask* is successful, the result of applying the *mapper* function. If the awaited *resultTask* is a failure, the original error is propagated.

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Result](../Result.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.BindAsync&lt;TOut&gt; method (3 of 6)

Asynchronously binds a Task to a new [`Result`](../Result-1.md) by applying an asynchronous function. This method awaits the input Task and then applies an asynchronous binding function if the awaited result is successful, allowing for propagation of success or failure.

```csharp
public static Task<Result<TOut>> BindAsync<TOut>(this Task<Result> resultTask, 
    Func<Task<Result<TOut>>> mapper)
```

| parameter | description |
| --- | --- |
| TOut | The type of the value contained in the output [`Result`](../Result-1.md). |
| resultTask | The input Task representing an asynchronous operation that yields a non-generic result. |
| mapper | The asynchronous function to apply if the awaited *resultTask* is successful. This function takes no arguments and returns a [`Result`](../Result-1.md). |

## Return Value

A [`Result`](../Result-1.md) representing the asynchronous operation: If the awaited *resultTask* is successful, the result of applying the *mapper* function. If the awaited *resultTask* is a failure, a [`Result`](../Result-1.md) containing the original error is returned.

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Result](../Result.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.BindAsync&lt;TIn,TOut&gt; method (4 of 6)

Asynchronously binds a [`Result`](../Result-1.md) to a new [`Result`](../Result-1.md) by applying an asynchronous function to its contained value. This method is used to chain asynchronous operations where the next step (defined by the *mapper*) takes the successful value of the current result and returns a new [`Result`](../Result-1.md), allowing for propagation of success or failure.

```csharp
public static Task<Result<TOut>> BindAsync<TIn, TOut>(this Result<TIn> result, 
    Func<TIn, Task<Result<TOut>>> mapper)
```

| parameter | description |
| --- | --- |
| TIn | The type of the value contained in the input [`Result`](../Result-1.md). |
| TOut | The type of the value contained in the output [`Result`](../Result-1.md). |
| result | The input [`Result`](../Result-1.md). |
| mapper | The asynchronous function to apply if the input *result* is successful. This function takes the successful value of type *TIn* and returns a [`Result`](../Result-1.md). |

## Return Value

A [`Result`](../Result-1.md) representing the asynchronous operation: If the input *result* is successful, the result of applying the *mapper* function to its value. If the input *result* is a failure, a [`Result`](../Result-1.md) containing the original error is returned.

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.BindAsync&lt;TIn,TOut&gt; method (5 of 6)

Asynchronously binds a [`Result`](../Result-1.md) to a new [`Result`](../Result-1.md) by applying a synchronous function to its contained value. This method awaits the input [`Result`](../Result-1.md) and then applies a synchronous binding function if the awaited result is successful, allowing for propagation of success or failure.

```csharp
public static Task<Result<TOut>> BindAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, 
    Func<TIn, Result<TOut>> mapper)
```

| parameter | description |
| --- | --- |
| TIn | The type of the value contained in the input [`Result`](../Result-1.md). |
| TOut | The type of the value contained in the output [`Result`](../Result-1.md). |
| resultTask | The input [`Result`](../Result-1.md) representing an asynchronous operation that yields a generic result. |
| mapper | The synchronous function to apply if the awaited *resultTask* is successful. This function takes the successful value of type *TIn* and returns a [`Result`](../Result-1.md). |

## Return Value

A [`Result`](../Result-1.md) representing the asynchronous operation: If the awaited *resultTask* is successful, the result of applying the *mapper* function to its value. If the awaited *resultTask* is a failure, the original error is propagated.

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

---

# ResultExtensions.BindAsync&lt;TIn,TOut&gt; method (6 of 6)

Asynchronously binds a [`Result`](../Result-1.md) to a new [`Result`](../Result-1.md) by applying an asynchronous function to its contained value. This method awaits the input [`Result`](../Result-1.md) and then applies an asynchronous binding function if the awaited result is successful, allowing for propagation of success or failure.

```csharp
public static Task<Result<TOut>> BindAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, 
    Func<TIn, Task<Result<TOut>>> mapper)
```

| parameter | description |
| --- | --- |
| TIn | The type of the value contained in the input [`Result`](../Result-1.md). |
| TOut | The type of the value contained in the output [`Result`](../Result-1.md). |
| resultTask | The input [`Result`](../Result-1.md) representing an asynchronous operation that yields a generic result. |
| mapper | The asynchronous function to apply if the awaited *resultTask* is successful. This function takes the successful value of type *TIn* and returns a [`Result`](../Result-1.md). |

## Return Value

A [`Result`](../Result-1.md) representing the asynchronous operation: If the awaited *resultTask* is successful, the result of applying the *mapper* function to its value. If the awaited *resultTask* is a failure, a [`Result`](../Result-1.md) containing the original error is returned.

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../README.md)

<!-- DO NOT EDIT: generated by xmldocmd for LightningArc.Results.dll -->
