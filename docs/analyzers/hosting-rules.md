# Hosting Rules

Rule LARC080 covers hosted-service scaffolding. Follow the link for the full description, examples, suppression, and limitations.

---

## LARC080

**`HostedService` `StartAsync` performs no work** — Info — Code fix: no

A class implementing `IHostedService` whose `StartAsync` body is only `return Task.CompletedTask;` (block-bodied) or `=> Task.CompletedTask;` (arrow-bodied). The service starts but does nothing — usually a stub that was never implemented or leftover scaffolding. A no-op `StartAsync` is sometimes intentional during stubbing, hence Info severity.

Details: [LARC080](LARC080.md)
