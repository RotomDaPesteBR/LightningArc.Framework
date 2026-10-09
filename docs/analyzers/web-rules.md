# Web Rules

Rule LARC060 covers `Result` mapping in ASP.NET Core Minimal APIs. Follow the link for the full description, examples, suppression, and limitations.

---

## LARC060

**Missing `.ToEndpointResult()` in Minimal API** — Warning — Code fix: yes

A Minimal API `MapGet`/`MapPost`/`MapPut`/`MapDelete`/`MapPatch`/`MapMethods` handler lambda returning `Result`/`Result<T>` without calling `.ToEndpointResult()`. Without it the `Result` is default-JSON-serialized instead of being mapped to the correct HTTP status code. The fix appends `.ToEndpointResult()` to the lambda's returned expression (one edit per `return` for block-bodied lambdas).

Details: [LARC060](LARC060.md)
