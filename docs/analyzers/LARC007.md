# LARC007 — ResultAggregator built with no checks

**Severity:** Info  **Domain:** Result  **Code fix:** Yes (sync only)

## What it detects

A `Result.Aggregate()` fluent chain that goes straight to `Build()`/`BuildAsync()` with no `Check`/`CheckEach`/`CheckAsync`/`CheckAll`/`Ensure`/`When`/`WhenAsync`/`WhenAll` call in between. Such a chain always succeeds — it is either dead code or a check that was removed or forgotten.

## ❌ Bad

```csharp
Result result = Result.Aggregate().Build(); // warns: no checks, always succeeds
```

## ✅ Good

```csharp
Result result = Result.Aggregate()
    .Check(() => ValidateName(name))
    .Ensure(age >= 0, Error.Validation.OutOfRange("age"))
    .Build(); // a check is present: no warning
```

## Code fix

Available for the sync case only: a check-less `Result.Aggregate().Build()` chain is replaced with `Result.Success()`, which is what the empty chain always evaluates to. No fix is ever offered for `BuildAsync()` — the async chain cannot be proven empty from syntax alone, so rewriting it would risk discarding asynchronous work.

## Suppression

```csharp
#pragma warning disable LARC007
Result result = Result.Aggregate().Build();
#pragma warning restore LARC007
```

```ini
# .editorconfig
dotnet_diagnostic.LARC007.severity = none
```

## Scope & limitations

Deliberate bail-outs (false negatives by design): if the chain contains any unrecognized method the analyzer stops reasoning and stays silent; if `Build()` is called on a stored variable (`var a = Result.Aggregate(); a.Build();`) it is not flagged, since proving emptiness across a variable boundary would need dataflow analysis. An unrelated type with `Aggregate()`/`Build()` members, or a compilation without `LightningArc.Results`, never triggers the rule.

## Related

- [LARC008](LARC008.md) — `Error` accumulated via `+=` in a loop
- [LARC006](LARC006.md) — redundant try-catch for Result mapping
