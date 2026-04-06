# Infra Rules

Rules LARC020, LARC022, and LARC030 cover database connection lifecycle and framework-specific warnings.

---

## LARC020

**Synchronous `GetConnection()` used in async method**

Calling the sync `GetConnection()` inside an `async` method defeats the purpose of async. Use `GetConnectionAsync()` instead.

### ❌ Bad

```csharp
public async Task<User> GetByIdAsync(int id, CancellationToken ct = default)
{
    var connection = GetConnection(); // blocks
}
```

### ✅ Good

```csharp
public async Task<User> GetByIdAsync(int id, CancellationToken ct = default)
{
    var connection = await GetConnectionAsync(ct);
}
```

---

## LARC022

**`IHostedService.StartAsync` performs no work**

A `IHostedService` implementation whose `StartAsync` method only returns `Task.CompletedTask` likely has a setup issue — either it should do work or not implement `IHostedService`.

### ❌ Bad

```csharp
public class MyService : IHostedService
{
    public Task StartAsync(CancellationToken ct) => Task.CompletedTask;
}
```

### ✅ Good

```csharp
public class MyService : IHostedService
{
    public Task StartAsync(CancellationToken ct)
    {
        // Actual initialization work
        return Task.CompletedTask;
    }
}
```

> This is an **Info**-level rule — a no-op `StartAsync` is sometimes intentional during stubbing.

---

## LARC030

**`ReleaseConnection(null)` has no effect**

Calling `ReleaseConnection` with a null argument does nothing. This usually indicates a bug — you likely meant to pass the actual connection variable.

### ❌ Bad

```csharp
ReleaseConnection(null); // no-op
```

### ✅ Good

```csharp
ReleaseConnection(connection); // actual cleanup
```
