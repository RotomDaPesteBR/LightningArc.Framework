# ResultTypeFactory.cs Implementation Details

**File Path:** `Meta/Metalama/Factories/Factories/ResultTypeFactory.cs`
**Namespace:** `LightningArc.Metalama`

## Overview
Similar to `TaskTypeFactory`, but specialized for the `Result` and `Result<T>` types defined in `Results`.

## Code Analysis

### `GetType`
Generates references to `Result` or `Result<T>`.

### `GetTaskType`
Combines `TaskTypeFactory` and `ResultTypeFactory` to generate references to `Task<Result>` or `Task<Result<T>>`. This is extremely useful for aspects that need to wrap methods returning domain Results in async Tasks.
