# LARC080 — HostedService StartAsync performs no work

**Severity:** Info  **Domain:** Infra  **Code fix:** No

## What it detects

A class implementing `IHostedService` whose `StartAsync` body is only `return Task.CompletedTask;` (block-bodied) or `=> Task.CompletedTask;` (arrow-bodied). The service starts but does nothing — usually a stub that was never implemented or leftover scaffolding.

## ❌ Bad

```csharp
public class SyncWorker : IHostedService
{
    public Task StartAsync(CancellationToken ct) // warns: no work performed
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
```

## ✅ Good

```csharp
public class SyncWorker : IHostedService
{
    public Task StartAsync(CancellationToken ct)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, _interval); // real work: no warning
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
```

## Code fix

None is offered deliberately: the "fix" is writing the service's business logic, which only the author can do (see GAP-11 "deliberately no fix" list).

## Suppression

```csharp
#pragma warning disable LARC080
public Task StartAsync(CancellationToken ct) => Task.CompletedTask;
#pragma warning restore LARC080
```

```ini
# .editorconfig
dotnet_diagnostic.LARC080.severity = none
```

## Scope & limitations

Only methods named exactly `StartAsync` in a class whose symbols implement `IHostedService` (by interface walk) are examined. Only the single-statement shape counts: one `return Task.CompletedTask;` (or the arrow equivalent, including `ValueTask` variants). Any additional statement — a field assignment, a method call, anything — silences the rule.

## Related

- [LARC006](LARC006.md) — redundant try-catch for Result mapping (background services are outside its compilation gate)
