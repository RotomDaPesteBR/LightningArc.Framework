# LightningArc.Analyzers

This project provides Roslyn analyzers that enforce best practices across the LightningArc ecosystem. Rules are evaluated at compile time — you will see warnings and errors directly in your IDE and during CI builds.

## Installation

All library projects in the solution automatically reference `LightningArc.Analyzers` via `src/Directory.Build.props`. No manual setup is needed for projects within the solution.

To use analyzers in an **external** project consuming LightningArc via NuGet, you will need to add the analyzer package once it is published.

## Rule Categories

| Prefix | Scope | IDs |
|---|---|---|
| **Result** | Safe usage of the Result pattern | LARC001–LARC003 |
| **ValueObject** | ValueObject creation and string conversion | LARC010–LARC012 |
| **Data** | Database connection lifecycle | LARC020, LARC030 |
| **Infra** | General framework warnings | LARC022 |

## Rules

### Result Rules

| ID | Title | Severity |
|---|---|---|
| [LARC001](result-rules.md#larc001) | Unsafe `.Value` access without `IsSuccess` check | Warning |
| [LARC002](result-rules.md#larc002) | Unsafe `.Error` access without `IsFailure` check | Warning |
| [LARC003](result-rules.md#larc003) | Result return value discarded | Info |

### ValueObject Rules

| ID | Title | Severity |
|---|---|---|
| [LARC010](value-object-rules.md#larc010) | Implicit string → ValueObject conversion may throw | Warning |
| [LARC011](value-object-rules.md#larc011) | Null ValueObject → string conversion may throw | Warning |
| [LARC012](value-object-rules.md#larc012) | ValueObject creation result discarded | Info |

### Data & Infra Rules

| ID | Title | Severity |
|---|---|---|
| [LARC020](infra-rules.md#larc020) | Synchronous `GetConnection()` used in async method | Warning |
| [LARC022](infra-rules.md#larc022) | `IHostedService.StartAsync` performs no work | Info |
| [LARC030](infra-rules.md#larc030) | `ReleaseConnection(null)` has no effect | Warning |

## Code Fixes

Two rules have automatic code fixes available in supported IDEs (Visual Studio, VS Code with C# Dev Kit):

| Rule | Fix Action |
|---|---|
| LARC001 | Wraps `.Value` access in `TryGetValue(out var value)` |
| LARC002 | Wraps `.Error` access in `TryGetError(out var error)` |
