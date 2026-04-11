## Design Philosophy

LightningArc was not designed as a full framework from the beginning.

It started as a simple utility library with a basic `Result` type and a single `Error` model containing only a message. This approach worked well initially, but as real-world applications evolved, limitations began to emerge.

### From Simple Errors to Domain Semantics

The first iteration of the error model introduced numeric codes directly tied to HTTP responses. While this solved short-term needs, it created a critical issue:

> Lower-level layers (such as data access) became aware of HTTP concerns.

This resulted in a **leaky abstraction**, where infrastructure details (HTTP status codes) started contaminating domain logic.

### Separation of Meaning and Representation

To solve this, LightningArc evolved its error model with a clear principle:

> Errors should represent **what happened**, not **how it is exposed**.

Instead of encoding HTTP semantics directly into errors, the framework now models errors as **domain or system concepts**, such as:

* `Resource.NotFound`
* `Validation.InvalidParameter`
* `Database.ConnectionFailed`

These errors are transport-agnostic and can be used in any environment.

### Mapping as a First-Class Concept

HTTP concerns are handled explicitly in the web layer through mapping:

* Domain errors → HTTP status codes
* Domain errors → ProblemDetails (`type`, `title`)
* Domain errors → API responses

This allows the same core logic to be reused across:

* ASP.NET Core APIs
* Background services
* Console applications
* Future integrations

### Consistency Through Composition

LightningArc is built around composable primitives:

* `Result<T>` for flow control
* `Error` for failure semantics
* `ErrorDetail` for contextual information

These components work together to create predictable and expressive application flows.

### Compile-Time Safety

The framework reinforces correct usage through Roslyn analyzers, ensuring:

* Safe access to `Result.Value`
* Proper handling of failure states
* Avoidance of dangerous implicit conversions

### Guiding Principle

LightningArc follows a simple rule:

> Make correct code easier to write than incorrect code.

By separating concerns, enforcing consistency, and providing structured abstractions, the framework enables developers to focus on business logic without leaking infrastructure details into the core of the application.
