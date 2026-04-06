# Result.cs Implementation Details

**File Path:** `Core/Results/Results/Result.cs`
**Namespace:** `LightningArc.Results`

## Overview
The `Result` class is the core component of the functional error handling pattern used in this library. It represents the outcome of an operation, which can either be a **Success** or a **Failure**. It avoids the use of exceptions for control flow and provides a type-safe way to handle errors.

## Code Analysis

### Base Class: `Result`

#### Properties
*   `IsSuccess`: Boolean indicating if the operation succeeded.
*   `IsFailure`: Boolean indicating if the operation failed (inverse of `IsSuccess`).
*   `Code`: Returns a generic status code (either from `SuccessDetails.Code` or `Error.Code`).
*   `Message`: Returns the associated message (either success or error message).
*   `Error`: Accessor for the `Error` object. Throws `ResultAccessFailedException` if accessed on a successful result.
*   `SuccessDetails`: Accessor for the `Success` object. Throws `ResultAccessFailedException` if accessed on a failed result.

#### Safe Access Methods
*   `TryGetError(out Error error)`: Attempt-free error retrieval. Returns `true` if the result is a failure; otherwise `false`. Prefer this over accessing `.Error` directly.

#### Internal State
*   `_error`: Nullable field holding the `Error` object (null if success).
*   `_success`: Nullable field holding the `Success` object (null if failure).

#### Constructors (Protected)
*   The constructors are `protected` to enforce the use of static factory methods.
*   They validate that `success` or `error` arguments are not null.

#### Static Factory Methods (Success)
*   `Success()`: Creates a generic success result (HTTP 200 OK equivalent).
*   `Success(Success success)`: Creates a result from a specific `Success` object.
*   `Created()`, `Accepted()`, `NoContent()`: Shortcuts for common success states.

#### Static Factory Methods (Failure)
*   `Failure(Error error)`: Creates a failure result from an `Error` object.

#### Implicit Operators
*   `implicit operator Result(Error error)`: Allows returning an `Error` directly where a `Result` is expected.
*   `implicit operator Result(Success success)`: Allows returning a `Success` directly where a `Result` is expected.

### Generic Class: `Result<TValue>`

Inherits from `Result` and adds a typed value to the success state.

#### Properties
*   `Value`: Accessor for the encapsulated value. Throws `ResultAccessFailedException` if accessed on a failed result.

#### Safe Access Methods
*   `TryGetValue(out TValue value)`: Attempt-free value retrieval. Returns `true` if the result is successful; otherwise `false`. Prefer this over accessing `.Value` directly. (LARC001 warns on unsafe `.Value` access.)

#### Factory Methods
*   `Success<TValue>(TValue value)`: Creates a success result with a value.
*   `Created<TValue>(TValue value)`: Creates a "Created" success result with a value.
*   `Accepted<TValue>(TValue value)`: Creates an "Accepted" success result with a value.
*   `NoContent<TValue>(TValue value)`: Creates a "No Content" success result with a value.

#### Implicit Operators
*   `implicit operator Result<TValue>(TValue value)`: Allows returning a raw value directly where a `Result<TValue>` is expected (defaults to OK).
*   `implicit operator Result<TValue>(Error error)`: Allows returning an `Error` directly.

#### Conversion
*   `ToResult(Result<TValue> result)`: Converts a generic result back to a non-generic `Result`, discarding the value but keeping the success/failure status and metadata.
