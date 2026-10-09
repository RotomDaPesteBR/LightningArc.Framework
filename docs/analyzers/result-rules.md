# Result Rules

Rules LARC001–LARC008 enforce safe usage of the Result pattern. Each section below summarizes the rule — follow the link for the full description, examples, suppression, and limitations.

---

## LARC001

**Unsafe access to `Result.Value`** — Warning — Code fix: yes

Reads of `.Value` on a `Result`/`Result<T>` that is not proven to be a success at that point. Accessing `.Value` on a failure throws, so a success guard or a `TryGetValue` call is required first. The fix wraps the access in a `TryGetValue` check.

Details: [LARC001](LARC001.md)

---

## LARC002

**Unsafe access to `Result.Error`** — Warning — Code fix: yes

Reads of `.Error` on a `Result`/`Result<T>` that is not proven to be a failure at that point. Accessing `.Error` on a success throws, so a failure guard or a `TryGetError` call is required first. The fix wraps the access in a `TryGetError` check.

Details: [LARC002](LARC002.md)

---

## LARC003

**Result value is discarded** — Info — Code fix: yes

An expression-statement invocation whose return value is a `Result`/`Result<T>` and is silently dropped. A discarded `Result` means a potential failure is never observed. The fix prefixes the statement with `_ = ` when the discard is deliberate.

Details: [LARC003](LARC003.md)

---

## LARC004

**`Result.Success(null)` for non-nullable type** — Warning — Code fix: no

`Result.Success(null)` (or `Result.Ok(null)`, including `default`/`default(T)` and `null!`) where the success type `T` is a non-nullable reference type. The resulting `Result<T>` claims a non-null value but holds null, defeating nullable-reference-type guarantees downstream. Only the author knows the real value (or whether `T` should be nullable), so no fix is offered.

Details: [LARC004](LARC004.md)

---

## LARC005

**Result error is shadowed** — Warning — Code fix: yes

A `return` that produces a new `Error`/`Result.Failure(...)` from inside an `if (x.IsFailure)` guard without ever consuming `x.Error`. The original error — and its traceability — is silently replaced. Consume the original error (log it or combine it with `+`) instead. The fix rewrites `return <newError>;` to `return <guard>.Error + <newError>;`.

Details: [LARC005](LARC005.md)

---

## LARC006

**Redundant try-catch for Result mapping** — Warning — Code fix: yes

A `try` with a single broad `catch` whose body contains exactly one statement returning an `Error`/`Result` factory. In a compilation that references `LightningArc.Results.AspNetCore.ResultExceptionHandler`, the global middleware already maps unhandled exceptions to standardized errors, so this local mapping is redundant. Only active in compilations referencing that middleware. The fix removes the try/catch and splices the try body in its place.

Details: [LARC006](LARC006.md)

---

## LARC007

**`ResultAggregator` built with no checks** — Info — Code fix: yes (sync only)

A `Result.Aggregate()` fluent chain that goes straight to `Build()`/`BuildAsync()` with no `Check`/`CheckEach`/`CheckAsync`/`CheckAll`/`Ensure`/`When`/`WhenAsync`/`WhenAll` call in between. Such a chain always succeeds — it is either dead code or a check that was removed or forgotten. The fix replaces the check-less sync `Result.Aggregate().Build()` chain with `Result.Success()`.

Details: [LARC007](LARC007.md)

---

## LARC008

**`Error` accumulated via `+=` inside a loop** — Info — Code fix: yes (no Fix-All)

An `errors += e;` where the target is `Error`-typed and sits inside a loop. Each `+=` re-flattens the accumulated `AggregateError` from scratch, making N accumulations roughly O(N²) work. Collect failures in a list and combine once with `Error.Aggregate`, or use `ResultAggregator.CheckAll` for concurrent checks. The fix rewrites the loop into that single-pass batch form (a `List<Error>` plus one `Error.Aggregate` call).

Details: [LARC008](LARC008.md)
