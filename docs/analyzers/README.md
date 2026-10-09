# LightningArc.Analyzers

This project provides Roslyn analyzers that enforce best practices across the LightningArc ecosystem. Rules are evaluated at compile time — you will see warnings and errors directly in your IDE and during CI builds.

## Installation

All library projects in the solution automatically reference `LightningArc.Analyzers` via `src/Directory.Build.props`. No manual setup is needed for projects within the solution.

To use analyzers in an **external** project consuming LightningArc via NuGet, you will need to add the analyzer package once it is published.

## Rule Categories

| Band | Scope | IDs | Details |
|---|---|---|---|
| **Result** | Safe usage of the Result pattern | LARC001–LARC008 | [result-rules.md](result-rules.md) |
| **ValueObject** | ValueObject creation, conversion, and shape | LARC020–LARC023 | [value-object-rules.md](value-object-rules.md) |
| **Data** | Repository connection lifecycle and transactions | LARC040–LARC044 | [data-rules.md](data-rules.md) |
| **Web** | Minimal API Result mapping | LARC060 | [web-rules.md](web-rules.md) |
| **Infra** | Hosted-service scaffolding | LARC080 | [hosting-rules.md](hosting-rules.md) |

## Rules

### Result Rules

| ID | Title | Severity |
|---|---|---|
| [LARC001](./LARC001.md) | Unsafe access to `Result.Value` | Warning |
| [LARC002](./LARC002.md) | Unsafe access to `Result.Error` | Warning |
| [LARC003](./LARC003.md) | Result value is discarded | Info |
| [LARC004](./LARC004.md) | `Result.Success(null)` for non-nullable type | Warning |
| [LARC005](./LARC005.md) | Result error is shadowed | Warning |
| [LARC006](./LARC006.md) | Redundant try-catch for Result mapping | Warning |
| [LARC007](./LARC007.md) | `ResultAggregator` built with no checks | Info |
| [LARC008](./LARC008.md) | `Error` accumulated via `+=` inside a loop | Info |

### ValueObject Rules

| ID | Title | Severity |
|---|---|---|
| [LARC020](./LARC020.md) | Implicit conversion from `string` to ValueObject | Warning |
| [LARC021](./LARC021.md) | Potential null ValueObject conversion to `string` | Warning |
| [LARC022](./LARC022.md) | ValueObject creation result is discarded | Info |
| [LARC023](./LARC023.md) | ValueObject should be a record | Warning |

### Data Rules

| ID | Title | Severity |
|---|---|---|
| [LARC040](./LARC040.md) | Synchronous `GetConnection` in async method | Warning |
| [LARC041](./LARC041.md) | Missing transaction in repository call | Info |
| [LARC042](./LARC042.md) | Direct `DbConnection` instantiation in repository | Warning |
| [LARC043](./LARC043.md) | `ReleaseConnection` called with null argument | Warning |
| [LARC044](./LARC044.md) | Missing `ReleaseConnection` after `GetConnection` | Warning |

### Web Rules

| ID | Title | Severity |
|---|---|---|
| [LARC060](./LARC060.md) | Missing `.ToEndpointResult()` in Minimal API | Warning |

### Infra Rules

| ID | Title | Severity |
|---|---|---|
| [LARC080](./LARC080.md) | `HostedService` `StartAsync` performs no work | Info |

## Code Fixes

Nine rules have automatic code fixes available in supported IDEs (Visual Studio, VS Code with C# Dev Kit):

| Rule | Fix Action |
|---|---|
| LARC001 | Wraps `.Value` access in `TryGetValue(out var value)` |
| LARC002 | Wraps `.Error` access in `TryGetError(out var error)` |
| LARC003 | Prefixes the statement with `_ = ` (explicit discard) |
| LARC006 | Removes the try/catch and splices the try body in its place |
| LARC021 | Rewrites the expression to `expr?.Value ?? string.Empty` |
| LARC022 | Prefixes the statement with `_ = ` (explicit discard) |
| LARC023 | Rewrites `class` to `record` |
| LARC040 | Rewrites to `await GetConnectionAsync(...).ConfigureAwait(false)` |
| LARC041 | Appends `transaction: Transaction` to the call |
