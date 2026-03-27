# TaskResult.cs Implementation Details

**File Path:** `Core/Results/Results/TaskResult.cs`
**Namespace:** `LightningArc.Results`

## Overview
`TaskResult<TValue>` is a wrapper around `Task<Result<TValue>>`. Its primary purpose is to simplify the syntax when working with asynchronous methods that return a `Result`.

## Code Analysis

### The Problem
Without this class, working with `Task<Result<T>>` often requires verbose syntax or double await calls in some chaining scenarios.

### The Solution: Custom Awaiter
The class implements the **Awaitable Pattern** by providing a `GetAwaiter()` method.
```csharp
public TaskResultAwaiter GetAwaiter() => new(_task);
```
This allows developers to `await` a `TaskResult<T>` directly, just like a standard `Task`.

### Implicit Conversions
```csharp
public static implicit operator TaskResult<TValue>(Task<Result<TValue>> task) => new(task);
```
This allows a method returning `TaskResult<T>` to simply return a `Task<Result<T>>` (or just an async method body) without explicit casting.

### Static Factory Class (`TaskResult`)
Provides helper methods to create pre-completed tasks:
*   `Success(value)`: Returns a completed task with a success result.
*   `Failure(error)`: Returns a completed task with a failure result.
*   `FromTask(Task<Result>)`: Converts a non-generic `Task<Result>` into a `TaskResult<object>`, enabling uniform handling of async results.
