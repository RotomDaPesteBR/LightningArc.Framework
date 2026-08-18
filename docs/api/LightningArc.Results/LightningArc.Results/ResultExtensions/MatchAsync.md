# ResultExtensions.MatchAsync method (1 of 14)

Maps a synchronous [`Result`](../Result.md) using a delegate that receives the original result.

```csharp
public static Task<Result> MatchAsync(this Result result, Func<Success, Task<Success>> success, 
    Func<Error, Task<Result>> failure)
```

## See Also

* class [Result](../Result.md)
* class [Success](../Success.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.MatchAsync method (2 of 14)

Maps a non-generic result (without value) using a synchronous success function, but with an asynchronous failure function. Returns a Task&lt;Result&gt;.

```csharp
public static Task<Result> MatchAsync(this Result result, Func<Success> success, 
    Func<Error, Task<Result>> failure)
```

| parameter | description |
| --- | --- |
| result | The input result (without value). |
| success | The synchronous function to apply on success, returning a [`Result`](../Result.md). |
| failure | The asynchronous function to apply on failure, returning a Task&lt;Result&gt;. |

## Return Value

A Task containing the new [`Result`](../Result.md).

## See Also

* class [Result](../Result.md)
* class [Success](../Success.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.MatchAsync method (3 of 14)

Maps a non-generic result (without value) by applying asynchronous functions, returning a Task&lt;Result&gt;.

```csharp
public static Task<Result> MatchAsync(this Result result, Func<Task<Result>> success, 
    Func<Error, Task<Result>> failure)
```

| parameter | description |
| --- | --- |
| result | The input result (without value). |
| success | The asynchronous function to apply on success, returning a Task&lt;Result&gt;. |
| failure | The asynchronous function to apply on failure, returning a Task&lt;Result&gt;. |

## Return Value

A Task containing the new [`Result`](../Result.md).

## See Also

* class [Result](../Result.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.MatchAsync method (4 of 14)

Awaits a Task&lt;Result&gt; and maps the result. The success function receives the original [`Result`](../Result.md) (for re-sending), and failure receives the [`Error`](../Error.md), both returning Task&lt;Result&gt;.

```csharp
public static Task<Result> MatchAsync(this Task<Result> resultTask, 
    Func<Success, Task<Success>> success, Func<Error, Task<Result>> failure)
```

## See Also

* class [Result](../Result.md)
* class [Success](../Success.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.MatchAsync method (5 of 14)

Awaits a Task&lt;Result&gt; and maps the result using synchronous functions for success, with an asynchronous failure function. Returns a Task&lt;Result&gt;.

```csharp
public static Task<Result> MatchAsync(this Task<Result> resultTask, Func<Success> success, 
    Func<Error, Task<Result>> failure)
```

| parameter | description |
| --- | --- |
| resultTask | The Task containing the input result. |
| success | The synchronous function to apply on success, returning a [`Result`](../Result.md). |
| failure | The asynchronous function to apply on failure, returning a Task&lt;Result&gt;. |

## Return Value

A Task containing the new [`Result`](../Result.md).

## See Also

* class [Result](../Result.md)
* class [Success](../Success.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.MatchAsync method (6 of 14)

Awaits a Task&lt;Result&gt; and maps the result using asynchronous functions, returning a Task&lt;Result&gt;.

```csharp
public static Task<Result> MatchAsync(this Task<Result> resultTask, Func<Task<Success>> success, 
    Func<Error, Task<Result>> failure)
```

| parameter | description |
| --- | --- |
| resultTask | The Task containing the input result. |
| success | The asynchronous function to apply on success, returning a Task&lt;Result&gt;. |
| failure | The asynchronous function to apply on failure, returning a Task&lt;Result&gt;. |

## Return Value

A Task containing the new [`Result`](../Result.md).

## See Also

* class [Result](../Result.md)
* class [Success](../Success.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.MatchAsync&lt;TOut&gt; method (7 of 14)

Maps a non-generic result to a generic result by applying asynchronous functions.

```csharp
public static Task<Result<TOut>> MatchAsync<TOut>(this Result result, 
    Func<Success, Task<Result<TOut>>> success, Func<Error, Task<Result<TOut>>> failure)
```

| parameter | description |
| --- | --- |
| TOut | The type of the output value. |
| result | The input result. |
| success | The asynchronous function to apply on success. |
| failure | The asynchronous function to apply on failure. |

## Return Value

A Task&lt;Result&lt;TOut&gt;&gt; containing the new [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Result](../Result.md)
* class [Success](../Success.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.MatchAsync&lt;TOut&gt; method (8 of 14)

Maps a non-generic result to a generic result by applying asynchronous functions.

```csharp
public static Task<Result<TOut>> MatchAsync<TOut>(this Result result, 
    Func<Success, Task<Success<TOut>>> success, Func<Error, Task<Result<TOut>>> failure)
```

| parameter | description |
| --- | --- |
| TOut | The type of the output value. |
| result | The input result. |
| success | The asynchronous function to apply on success. |
| failure | The asynchronous function to apply on failure. |

## Return Value

A Task&lt;Result&lt;TOut&gt;&gt; containing the new [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Result](../Result.md)
* class [Success](../Success.md)
* class [Success&lt;TValue&gt;](../Success-1.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.MatchAsync&lt;TOut&gt; method (9 of 14)

Awaits a Task&lt;Result&gt; and maps the result using asynchronous functions, where success returns a [`Result`](../Result-1.md). The success function receives the success details ([`Success`](../Success.md)).

```csharp
public static Task<Result<TOut>> MatchAsync<TOut>(this Task<Result> resultTask, 
    Func<Success, Task<Success<TOut>>> success, Func<Error, Task<Result<TOut>>> failure)
```

| parameter | description |
| --- | --- |
| TOut | The type of the output value (if successful). |
| resultTask | The Task containing the input result. |
| success | The asynchronous function to apply to the success details, which returns a Task&lt;Result&lt;TOut&gt;&gt;. |
| failure | The asynchronous function to apply to the error. |

## Return Value

A Task&lt;Result&lt;TOut&gt;&gt; containing the new [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Result](../Result.md)
* class [Success](../Success.md)
* class [Success&lt;TValue&gt;](../Success-1.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.MatchAsync&lt;TIn,TOut&gt; method (10 of 14)

Maps the synchronous result ([`Result`](../Result-1.md)) using an asynchronous success function that returns a [`Result`](../Result-1.md) and a synchronous failure function.

```csharp
public static Task<Result<TOut>> MatchAsync<TIn, TOut>(this Result<TIn> result, 
    Func<Success<TIn>, Task<Success<TOut>>> success, Func<Error, Result<TOut>> failure)
```

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Success&lt;TValue&gt;](../Success-1.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.MatchAsync&lt;TIn,TOut&gt; method (11 of 14)

Maps the result by applying one of two asynchronous functions: one for success and one for failure.

```csharp
public static Task<Result<TOut>> MatchAsync<TIn, TOut>(this Result<TIn> result, 
    Func<Success<TIn>, Task<Success<TOut>>> success, Func<Error, Task<Result<TOut>>> failure)
```

| parameter | description |
| --- | --- |
| TIn | The type of the input value. |
| TOut | The type of the output value. |
| result | The input result. |
| success | The asynchronous function to apply to the success details ([`Success`](../Success-1.md)). |
| failure | The asynchronous function to apply to the error. |

## Return Value

A Task&lt;Result&lt;TOut&gt;&gt;

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Success&lt;TValue&gt;](../Success-1.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.MatchAsync&lt;TIn,TOut&gt; method (12 of 14)

Awaits a Task&lt;Result&lt;TIn&gt;&gt; and maps the result using synchronous functions for success, returning a [`Result`](../Result-1.md).

```csharp
public static Task<Result<TOut>> MatchAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, 
    Func<Success<TIn>, Success<TOut>> success, Func<Error, Result<TOut>> failure)
```

| parameter | description |
| --- | --- |
| TIn | The type of the input value. |
| TOut | The type of the output value. |
| resultTask | The Task containing the input result. |
| success | The synchronous function to apply to the success details ([`Success`](../Success-1.md)), returning [`Result`](../Result-1.md). |
| failure | The synchronous function to apply to the error, returning the failure [`Result`](../Result-1.md). |

## Return Value

A Task&lt;Result&lt;TOut&gt;&gt; containing the new [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Success&lt;TValue&gt;](../Success-1.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.MatchAsync&lt;TIn,TOut&gt; method (13 of 14)

Awaits the asynchronous input result and maps it using a synchronous success function (returns [`Result`](../Result-1.md)) and an asynchronous failure function.

```csharp
public static Task<Result<TOut>> MatchAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, 
    Func<Success<TIn>, Success<TOut>> success, Func<Error, Task<Result<TOut>>> failure)
```

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Success&lt;TValue&gt;](../Success-1.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.MatchAsync&lt;TIn,TOut&gt; method (14 of 14)

Awaits a Task&lt;Result&lt;TIn&gt;&gt; and maps the result using asynchronous functions for success and failure.

```csharp
public static Task<Result<TOut>> MatchAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, 
    Func<Success<TIn>, Task<Success<TOut>>> success, Func<Error, Task<Result<TOut>>> failure)
```

| parameter | description |
| --- | --- |
| TIn | The type of the input value. |
| TOut | The type of the output value. |
| resultTask | The Task containing the input result. |
| success | The asynchronous function to apply to the success details ([`Success`](../Success-1.md)), returning Task&lt;Result&lt;TOut&gt;&gt;. |
| failure | The asynchronous function to apply to the error, returning the failure Task&lt;Result&lt;TOut&gt;&gt;. |

## Return Value

A Task&lt;Result&lt;TOut&gt;&gt; containing the new [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Success&lt;TValue&gt;](../Success-1.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

<!-- DO NOT EDIT: generated by xmldocmd for LightningArc.Results.dll -->
