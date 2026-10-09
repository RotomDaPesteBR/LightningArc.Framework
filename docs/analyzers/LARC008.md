# LARC008 — Error accumulated via '+=' inside a loop

**Severity:** Info  **Domain:** Result  **Code fix:** Yes (no Fix-All)

## What it detects

An `errors += e;` where the target is `Error`-typed and sits inside a `for`/`foreach`/`while`/`do` loop. Each `+=` re-flattens the accumulated `AggregateError` from scratch (see `Error.operator+`), making N accumulations roughly O(N²) work.

## ❌ Bad

```csharp
Error? errors = null;
foreach (var item in items)
{
    errors += Validate(item); // warns: O(N²) re-flattening
}
```

## ✅ Good

```csharp
var failures = new List<Error>();
foreach (var item in items)
{
    Result r = Validate(item);
    if (r.IsFailure)
    {
        failures.Add(r.Error);
    }
}
Error? errors = failures.Count == 0 ? null : Error.Aggregate(failures);
```

Or use the `ResultAggregator` fluent API (`Check`/`Ensure`/`When`/`CheckAll`/`WhenAll`), which accumulates in one pass.

## Code fix

Available. The fix rewrites the loop into the single-pass batch form — a `List<Error>` declared before the loop, `Add` calls inside it, and one `Error.Aggregate(list)` assignment after the loop (adding `using System.Collections.Generic;` when the file lacks it):

```csharp
Error? errors = null;
var errorsList = new List<Error>();
foreach (var item in items)
{
    errorsList.Add(Validate(item));
}
errors = Error.Aggregate(errorsList);
```

Withholding conditions (no fix is offered): the accumulator must be a local declared outside (and before) the loop — fields/properties/parameters never qualify; every other reference to it must sit after the loop (reads before or inside the loop, or any second write such as another loop's `+=`, decline the fix); the `+=` must be a standalone statement in the loop's own flow (not deferred inside a nested lambda/local function); and the loop must sit directly in a block. Fix-All is deliberately disabled for this rule: each multi-statement loop rewrite needs its own review.

## Suppression

```csharp
#pragma warning disable LARC008
foreach (var item in items) { errors += Validate(item); }
#pragma warning restore LARC008
```

```ini
# .editorconfig
dotnet_diagnostic.LARC008.severity = none
```

## Scope & limitations

Only `+=` on an `Error`-typed target inside a loop counts; `+=` on `int`/`string`/other types is ignored. The loop search does not cross method or local-function boundaries: a `+=` inside a method that merely happens to be *called from* a loop is not flagged (that would need call-graph analysis).

## Related

- [LARC007](LARC007.md) — `ResultAggregator` built with no checks
- [LARC005](LARC005.md) — shadowed `Result` error
