# ResultExtensions.Map&lt;TOut&gt; method (1 of 4)

Maps a non-generic [`Result`](../Result.md) to a new [`Result`](../Result-1.md) by applying a synchronous function that returns a [`Success`](../Success-1.md). This method is used when the operation represented by the *result* is successful, and you want to transform it into a new result containing a different type of value, while also providing custom success details. The transformation itself is assumed to be non-failable.

```csharp
public static Result<TOut> Map<TOut>(this Result result, Func<Success<TOut>> mapper)
```

| parameter | description |
| --- | --- |
| TOut | The type of the value contained in the output [`Result`](../Result-1.md). |
| result | The input non-generic [`Result`](../Result.md). |
| mapper | The synchronous function to apply if the input *result* is successful. This function takes no arguments and returns a [`Success`](../Success-1.md). |

## Return Value

A new [`Result`](../Result-1.md): If the input *result* is successful, a [`Result`](../Result-1.md) encapsulating the [`Success`](../Success-1.md) returned by the *mapper*. If the input *result* is a failure, the original error is propagated.

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Result](../Result.md)
* class [Success&lt;TValue&gt;](../Success-1.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.Map&lt;TOut&gt; method (2 of 4)

Maps a non-generic [`Result`](../Result.md) to a new [`Result`](../Result-1.md) by applying a synchronous transformation function. This method is used when the operation represented by the *result* is successful, and you want to transform it into a new result containing a different type of value. The transformation itself is assumed to be non-failable.

```csharp
public static Result<TOut> Map<TOut>(this Result result, Func<TOut> mapper)
```

| parameter | description |
| --- | --- |
| TOut | The type of the value contained in the output [`Result`](../Result-1.md). |
| result | The input non-generic [`Result`](../Result.md). |
| mapper | The synchronous function to apply if the input *result* is successful. This function takes no arguments and returns a value of type *TOut*. |

## Return Value

A new [`Result`](../Result-1.md): If the input *result* is successful, a [`Result`](../Result-1.md) containing the transformed value. If the input *result* is a failure, the original error is propagated.

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Result](../Result.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.Map&lt;TIn,TOut&gt; method (3 of 4)

Maps a [`Result`](../Result-1.md) to a new [`Result`](../Result-1.md) by applying a synchronous function that returns a [`Success`](../Success-1.md) to its contained value. This method is used when the operation represented by the *result* is successful, and you want to transform its contained value of type *TIn* into a new result containing a different type of value, while also providing custom success details. The transformation itself is assumed to be non-failable.

```csharp
public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, Success<TOut>> mapper)
```

| parameter | description |
| --- | --- |
| TIn | The type of the value contained in the input [`Result`](../Result-1.md). |
| TOut | The type of the value contained in the output [`Result`](../Result-1.md). |
| result | The input [`Result`](../Result-1.md). |
| mapper | The synchronous function to apply if the input *result* is successful. This function takes the successful value of type *TIn* and returns a [`Success`](../Success-1.md). |

## Return Value

A new [`Result`](../Result-1.md): If the input *result* is successful, a [`Result`](../Result-1.md) encapsulating the [`Success`](../Success-1.md) returned by the *mapper*. If the input *result* is a failure, the original error is propagated.

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [Success&lt;TValue&gt;](../Success-1.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

---

# ResultExtensions.Map&lt;TIn,TOut&gt; method (4 of 4)

Maps a [`Result`](../Result-1.md) to a new [`Result`](../Result-1.md) by applying a synchronous transformation function to its contained value. This method is used when the operation represented by the *result* is successful, and you want to transform its contained value of type *TIn* into a new value of type *TOut*. The transformation itself is assumed to be non-failable.

```csharp
public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper)
```

| parameter | description |
| --- | --- |
| TIn | The type of the value contained in the input [`Result`](../Result-1.md). |
| TOut | The type of the value contained in the output [`Result`](../Result-1.md). |
| result | The input [`Result`](../Result-1.md). |
| mapper | The synchronous function to apply if the input *result* is successful. This function takes the successful value of type *TIn* and returns a value of type *TOut*. |

## Return Value

A new [`Result`](../Result-1.md): If the input *result* is successful, a [`Result`](../Result-1.md) containing the transformed value. If the input *result* is a failure, the original error is propagated.

## See Also

* class [Result&lt;TValue&gt;](../Result-1.md)
* class [ResultExtensions](../ResultExtensions.md)
* namespace [LightningArc.Results](../../LightningArc.Results.md)

<!-- DO NOT EDIT: generated by xmldocmd for LightningArc.Results.dll -->
