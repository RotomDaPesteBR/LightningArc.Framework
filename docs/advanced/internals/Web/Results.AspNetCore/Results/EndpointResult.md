# EndpointResult.cs Implementation Details

**File Path:** `Web/Results.AspNetCore/Results/EndpointResult.cs`
**Namespace:** `LightningArc.Results.AspNetCore`

## Overview
The `EndpointResult` classes (generic and non-generic) serve as **Adapters** between the domain's Functional Result pattern (`Result<T>`) and ASP.NET Core's `IResult` interface. This allows domain results to be returned directly from Minimal APIs or Controllers.

## Code Analysis

### `EndpointResult` (Non-Generic)
*   **Implements**: `Microsoft.AspNetCore.Http.IResult`.
*   **Role**: Handles operations that do not return a value (void/Task).
*   **Implicit Operator**:
    *   `implicit operator EndpointResult(Result result)`: Converts a domain `Result` into an `EndpointResult`.
    *   **Logic**:
        *   If Success: Wraps it in a `SuccessResult` (which maps to 200/201/204, etc.).
        *   If Failure: Wraps it in an `ErrorResult` (which maps to Problem Details).

### `EndpointResult<TValue>` (Generic)
*   **Implements**: `Microsoft.AspNetCore.Http.IResult`.
*   **Role**: Handles operations that return data.
*   **Implicit Operators**:
    *   `operator(Result<TValue>)`: Main converter.
    *   `operator(TValue)`: Shortcut to return a raw value as a Success Result.
    *   `operator(Error)`: Shortcut to return an Error as a Failure Result.
*   **ExecuteAsync**: Delegates the execution to the internal `_result` (which is either a `SuccessResult` or `ErrorResult` determined at construction).

### Design Pattern
This implementation allows for a very clean Controller/Endpoint syntax where the developer just returns the result of the service call, and the infrastructure handles the HTTP translation.
