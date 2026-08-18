# ResultExtensions class

Provides extension methods for the [`Result`](./Result-1.md) class, enabling the composition of functional operations.

Provides asynchronous extension methods for the [`Result`](./Result-1.md) class, focused on the chaining (Bind) of asynchronous operations.

Provides extension methods for the [`Result`](./Result-1.md) class, enabling the composition of functional operations.

Provides asynchronous extension methods for the [`Result`](./Result-1.md) class, focused on condition validation (Ensure) within an asynchronous flow.

Provides extension methods for the [`Result`](./Result-1.md) class, allowing functional composition of operations.

Provides extension methods for the [`Result`](./Result-1.md) class, allowing the composition of functional operations.

Provides extension methods for the [`Result`](./Result.md) and [`Result`](./Result-1.md) classes, focused on the transformation (Map) of the internal success value.

Provides asynchronous extension methods for the [`Result`](./Result.md) and [`Result`](./Result-1.md) classes, focused on the transformation (Map) of the internal success value.

Provides extension methods for the [`Result`](./Result-1.md) class, allowing the composition of functional operations.

Provides extension methods for the [`Result`](./Result-1.md) class, allowing the composition of functional operations.

Provides extension methods for the [`Result`](./Result.md) and [`Result`](./Result-1.md) classes, enabling the composition of functional operations (e.g., mapping, unwrapping).

Provides extension methods for the [`Result`](./Result.md) and [`Result`](./Result-1.md) classes, enabling the composition of functional and asynchronous operations.

Extension methods for Bind operations, such as `MatchBindAsync`, which chain an operation that returns a new [`Result`](./Result.md) or Task&lt;Result&gt;.

Provides extension methods for the [`Result`](./Result.md) and [`Result`](./Result-1.md) classes, enabling the composition of functional operations.

Provides asynchronous extension methods for the [`Result`](./Result.md) and [`Result`](./Result-1.md) classes, focused on handling failures within an asynchronous flow.

Provides extension methods for the [`Result`](./Result.md) and [`Result`](./Result-1.md) classes, enabling the composition of functional operations.

Provides asynchronous extension methods for the [`Result`](./Result.md) and [`Result`](./Result-1.md) classes, focused on executing side effects (Tap) within an asynchronous flow.

```csharp
public static class ResultExtensions
```

## Public Members

| name | description |
| --- | --- |
| static [Bind&lt;TOut&gt;](ResultExtensions/Bind.md)(…) | Binds a non-generic [`Result`](./Result.md) to a new [`Result`](./Result-1.md) by applying a function. This method is used to chain operations where the next step (defined by the *mapper*) also returns a [`Result`](./Result-1.md), allowing for propagation of success or failure. |
| static [Bind&lt;TIn,TOut&gt;](ResultExtensions/Bind.md)(…) | Binds a [`Result`](./Result-1.md) to a new [`Result`](./Result-1.md) by applying a function to its contained value. This method is used to chain operations where the next step (defined by the *mapper*) takes the successful value of the current result and returns a new [`Result`](./Result-1.md), allowing for propagation of success or failure. |
| static [BindAsync&lt;TOut&gt;](ResultExtensions/BindAsync.md)(…) | Asynchronously binds a non-generic [`Result`](./Result.md) to a new [`Result`](./Result-1.md) by applying an asynchronous function. This method is used to chain asynchronous operations where the next step (defined by the *mapper*) also returns a [`Result`](./Result-1.md), allowing for propagation of success or failure. (3 methods) |
| static [BindAsync&lt;TIn,TOut&gt;](ResultExtensions/BindAsync.md)(…) | Asynchronously binds a [`Result`](./Result-1.md) to a new [`Result`](./Result-1.md) by applying an asynchronous function to its contained value. This method is used to chain asynchronous operations where the next step (defined by the *mapper*) takes the successful value of the current result and returns a new [`Result`](./Result-1.md), allowing for propagation of success or failure. (3 methods) |
| static [Ensure](ResultExtensions/Ensure.md)(…) | Ensures that the given condition is true, otherwise returns a new failure [`Result`](./Result.md). (2 methods) |
| static [Ensure&lt;T&gt;](ResultExtensions/Ensure.md)(…) | Ensures that the given predicate is true, otherwise returns a new failure [`Result`](./Result.md). (2 methods) |
| static [Ensure&lt;TValue&gt;](ResultExtensions/Ensure.md)(…) | Checks a condition and, if it is false, transforms the successful result into a failed result using the provided error. Does nothing if an existing failure is present. This is the synchronous **Ensure** operation for validation checks. |
| static [EnsureAsync](ResultExtensions/EnsureAsync.md)(…) | Asynchronously ensures that the given condition is true, otherwise returns a new failure [`Result`](./Result.md). (3 methods) |
| static [EnsureAsync&lt;T&gt;](ResultExtensions/EnsureAsync.md)(…) | Asynchronously ensures that the given predicate is true, otherwise returns a new failure [`Result`](./Result.md). (3 methods) |
| static [GetValueOrDefault&lt;TValue&gt;](ResultExtensions/GetValueOrDefault.md)(…) | Returns the success value contained in the [`Result`](./Result-1.md). If the result is a failure, returns the provided default value. (3 methods) |
| static [GetValueOrDefaultAsync&lt;T&gt;](ResultExtensions/GetValueOrDefaultAsync.md)(…) | Asynchronously returns the value of a [`Result`](./Result-1.md) if it is a success, otherwise invokes an asynchronous factory to create a default value. (5 methods) |
| static [Map&lt;TOut&gt;](ResultExtensions/Map.md)(…) | Maps a non-generic [`Result`](./Result.md) to a new [`Result`](./Result-1.md) by applying a synchronous transformation function. This method is used when the operation represented by the *result* is successful, and you want to transform it into a new result containing a different type of value. The transformation itself is assumed to be non-failable. (2 methods) |
| static [Map&lt;TIn,TOut&gt;](ResultExtensions/Map.md)(…) | Maps a [`Result`](./Result-1.md) to a new [`Result`](./Result-1.md) by applying a synchronous transformation function to its contained value. This method is used when the operation represented by the *result* is successful, and you want to transform its contained value of type *TIn* into a new value of type *TOut*. The transformation itself is assumed to be non-failable. (2 methods) |
| static [MapAsync&lt;TOut&gt;](ResultExtensions/MapAsync.md)(…) | Asynchronously maps a non-generic [`Result`](./Result.md) to a new [`Result`](./Result-1.md) by applying an asynchronous transformation function. This method is used when the operation represented by the *result* is successful, and you want to transform it into a new result containing a different type of value. The transformation itself is assumed to be non-failable. (6 methods) |
| static [MapAsync&lt;TIn,TOut&gt;](ResultExtensions/MapAsync.md)(…) | Asynchronously maps a [`Result`](./Result-1.md) to a new [`Result`](./Result-1.md) by applying an asynchronous transformation function to its contained value. This method is used when the operation represented by the *result* is successful, and you want to transform its contained value of type *TIn* into a new value of type *TOut*. The transformation itself is assumed to be non-failable. (6 methods) |
| static [MapError](ResultExtensions/MapError.md)(…) | Maps the error of a [`Result`](./Result.md) to a new [`Error`](./Error.md). |
| static [MapError&lt;TValue&gt;](ResultExtensions/MapError.md)(…) | Maps the error of a [`Result`](./Result-1.md) to a new [`Error`](./Error.md). |
| static [MapErrorAsync](ResultExtensions/MapErrorAsync.md)(…) | Asynchronously maps the error of a [`Result`](./Result.md) to a new [`Error`](./Error.md). (3 methods) |
| static [MapErrorAsync&lt;TValue&gt;](ResultExtensions/MapErrorAsync.md)(…) | Asynchronously maps the error of a [`Result`](./Result-1.md) to a new [`Error`](./Error.md). (3 methods) |
| static [Match](ResultExtensions/Match.md)(…) | Maps a non-generic result (without value) to another non-generic [`Result`](./Result.md). (2 methods) |
| static [Match&lt;TOut&gt;](ResultExtensions/Match.md)(…) | Maps a non-generic result (without value) to a new [`Result`](./Result-1.md). (3 methods) |
| static [Match&lt;TIn,TOut&gt;](ResultExtensions/Match.md)(…) | Maps a generic result to a new [`Result`](./Result-1.md) where the success function returns the raw *TOut* value, which is then wrapped in a [`Result`](./Result-1.md). (3 methods) |
| static [MatchAsync](ResultExtensions/MatchAsync.md)(…) | Maps a non-generic result (without value) by applying asynchronous functions, returning a Task&lt;Result&gt;. (6 methods) |
| static [MatchAsync&lt;TOut&gt;](ResultExtensions/MatchAsync.md)(…) | Awaits a Task&lt;Result&gt; and maps the result using asynchronous functions, where success returns a [`Result`](./Result-1.md). The success function receives the success details ([`Success`](./Success.md)). (3 methods) |
| static [MatchAsync&lt;TIn,TOut&gt;](ResultExtensions/MatchAsync.md)(…) | Maps the result by applying one of two asynchronous functions: one for success and one for failure. (5 methods) |
| static [MatchBindAsync](ResultExtensions/MatchBindAsync.md)(…) | Maps a non-generic result (without value) by applying asynchronous functions, returning a Task&lt;Result&gt;. (6 methods) |
| static [MatchBindAsync&lt;TOut&gt;](ResultExtensions/MatchBindAsync.md)(…) | Awaits a Task&lt;Result&gt; and maps the result using asynchronous functions, where success returns a [`Result`](./Result-1.md). The success function receives the success details ([`Success`](./Success.md)). |
| static [MatchBindAsync&lt;TIn,TOut&gt;](ResultExtensions/MatchBindAsync.md)(…) | Awaits a Task&lt;Result&lt;TIn&gt;&gt; and maps the result. The success function receives the original [`Result`](./Result-1.md) (for re-sending or inspection), while failure receives the [`Error`](./Error.md). (7 methods) |
| static [OnFailure](ResultExtensions/OnFailure.md)(…) | Executes the given action if the [`Result`](./Result.md) is a failure. (2 methods) |
| static [OnFailure&lt;T&gt;](ResultExtensions/OnFailure.md)(…) | Executes the given action if the [`Result`](./Result-1.md) is a failure. (2 methods) |
| static [OnFailureAsync](ResultExtensions/OnFailureAsync.md)(…) | Asynchronously executes the given action if the [`Result`](./Result.md) is a failure. (6 methods) |
| static [OnFailureAsync&lt;T&gt;](ResultExtensions/OnFailureAsync.md)(…) | Asynchronously executes the given action if the [`Result`](./Result-1.md) is a failure. (6 methods) |
| static [Tap](ResultExtensions/Tap.md)(…) | Executes the given action if the [`Result`](./Result.md) is a success. |
| static [Tap&lt;T&gt;](ResultExtensions/Tap.md)(…) | Executes the given action if the [`Result`](./Result-1.md) is a success. (2 methods) |
| static [TapAsync](ResultExtensions/TapAsync.md)(…) | Asynchronously executes the given action if the [`Result`](./Result.md) is a success. (3 methods) |
| static [TapAsync&lt;T&gt;](ResultExtensions/TapAsync.md)(…) | Asynchronously executes the given action if the [`Result`](./Result-1.md) is a success. (6 methods) |

## See Also

* namespace [LightningArc.Results](../LightningArc.Results.md)
* [ResultExtensions.cs](https://github.com/RotomDaPesteBR/LightningArc.Framework/tree/dev/ResultExtensions.cs)

<!-- DO NOT EDIT: generated by xmldocmd for LightningArc.Results.dll -->
