# LARC008 — Error accumulated via '+=' inside a loop

**Severity:** Info  **Domain:** Result  **Code fix:** No

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

None yet. A future fix may rewrite the loop to collect into a `List<Error>` and call `Error.Aggregate(list)` after the loop — only when the accumulator is a local declared outside the loop and used only after it (see GAP-11).

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
