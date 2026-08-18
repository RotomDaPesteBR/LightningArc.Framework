# ResultExtensions.Match method (1 of 8)

Maps a non-generic result (without value) to another non-generic [`Result`](../Result.md).

```csharp
public static Result Match(this Result result, Func<Result> success, Func<Error, Result> failure)
```

| parameter | description |
| --- | --- |
| result | The input result (without value). |
| success | The function to apply on success, returning [`Result`](../Result.md). |
| failure | The function to apply on failure, returning [`Result`](../Result.md). |

## Return Value

The new mapped [`Result`](../Result.md).

## See Also

* class [Result](../Result.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.Match method (2 of 8)

Maps a non-generic result by applying one of two functions, where the success function receives the original success details ([`Success`](../Success.md) object).

```csharp
public static Result Match(this Result result, Func<Success, Result> success, 
    Func<Error, Result> failure)
```

| parameter | description |
| --- | --- |
| result | The input result (without value). |
| success | The function to apply to the success details, returning [`Result`](../Result.md). |
| failure | The function to apply on failure, returning [`Result`](../Result.md). |

## Return Value

The new mapped [`Result`](../Result.md).

## See Also

* class [Result](../Result.md)
* class [Success](../Success.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.Match&lt;TOut&gt; method (3 of 8)

Maps a non-generic result (without value) to a new [`Result`](../Result-1.md).

```csharp
public static Result<TOut> Match<TOut>(this Result result, Func<Result<TOut>> success, 
    Func<Error, Result<TOut>> failure)
```

| parameter | description |
| --- | --- |
| TOut | The type of the output value. |
| result | The input result (without value). |
| success | The function to apply on success, returning [`Result`](../Result-1.md). |
| failure | The function to apply on failure, returning [`Result`](../Result-1.md). |

## Return Value

The new mapped [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Result](../Result.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.Match&lt;TOut&gt; method (4 of 8)

Maps a non-generic result to a new [`Result`](../Result-1.md) where the success function returns the raw *TOut* value, which is then wrapped in a [`Result`](../Result-1.md).

```csharp
public static Result<TOut> Match<TOut>(this Result result, Func<TOut> success, 
    Func<Error, Result<TOut>> failure)
```

| parameter | description |
| --- | --- |
| TOut | The type of the output value. |
| result | The input result (without value). |
| success | The function to apply on success, returning the raw *TOut* value (which is automatically wrapped). |
| failure | The function to apply on failure, returning [`Result`](../Result-1.md). |

## Return Value

The new mapped [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Result](../Result.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.Match&lt;TOut&gt; method (5 of 8)

Unwraps a non-generic result (without value) by applying one of two functions, returning the final raw value *TOut*. This method ends the Result chain.

```csharp
public static TOut Match<TOut>(this Result result, Func<TOut> success, Func<Error, TOut> failure)
```

| parameter | description |
| --- | --- |
| TOut | The type of the output (unwrapped) value. |
| result | The input result (without value). |
| success | The function to apply on success, which returns *TOut*. |
| failure | The function to apply on failure, which returns *TOut*. |

## Return Value

The final *TOut* value.

## See Also

* class [Result](../Result.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.Match&lt;TIn,TOut&gt; method (6 of 8)

Maps a generic result by applying one of two functions: one for success and one for failure.

```csharp
public static Result<TOut> Match<TIn, TOut>(this Result<TIn> result, 
    Func<Success<TIn>, Result<TOut>> success, Func<Error, Result<TOut>> failure)
```

| parameter | description |
| --- | --- |
| TIn | The type of the input value. |
| TOut | The type of the output value. |
| result | The input result. |
| success | The function to apply to the success details, returning [`Result`](../Result-1.md). |
| failure | The function to apply on failure, returning [`Result`](../Result-1.md). |

## Return Value

The new mapped [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Success&lt;TValue&gt;](../Success-1.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.Match&lt;TIn,TOut&gt; method (7 of 8)

Maps a generic result to a new [`Result`](../Result-1.md) where the success function returns the raw *TOut* value, which is then wrapped in a [`Result`](../Result-1.md).

```csharp
public static Result<TOut> Match<TIn, TOut>(this Result<TIn> result, 
    Func<Success<TIn>, TOut> success, Func<Error, Result<TOut>> failure)
```

| parameter | description |
| --- | --- |
| TIn | The type of the input value. |
| TOut | The type of the output value. |
| result | The input result. |
| success | The function to apply to the success details, returning the raw *TOut* value (which is automatically wrapped). |
| failure | The function to apply on failure, returning [`Result`](../Result-1.md). |

## Return Value

The new mapped [`Result`](../Result-1.md).

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Success&lt;TValue&gt;](../Success-1.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.Match&lt;TIn,TOut&gt; method (8 of 8)

Unwraps a [`Result`](../Result-1.md) by applying one of two functions, returning the final raw value *TOut*. This method ends the [`Result`](../Result.md) chain.

```csharp
public static TOut Match<TIn, TOut>(this Result<TIn> result, Func<Success<TIn>, TOut> success, 
    Func<Error, TOut> failure)
```

| parameter | description |
| --- | --- |
| TIn | The type of the input value. |
| TOut | The type of the output (unwrapped) value. |
| result | The input result. |
| success | The function to apply to the success details, which returns *TOut*. |
| failure | The function to apply on failure, which returns *TOut*. |

## Return Value

The final *TOut* value.

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Success&lt;TValue&gt;](../Success-1.md)
* class [Error](../Error.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

<!-- DO NOT EDIT: generated by xmldocmd for LightningArc.Results.dll -->
