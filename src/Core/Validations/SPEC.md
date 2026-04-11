# LightningArc.Validation — Specification

## Overview

LightningArc.Validation is a validation library designed specifically for the LightningArc ecosystem.

It provides a fluent and expressive API for defining validation rules while integrating natively with:

* `LightningArc.Results`
* `Error.Validation.*`
* `ErrorDetail`
* ASP.NET Core mappings (via LightningArc.Results.AspNetCore)

Unlike general-purpose validation libraries, LightningArc.Validation is built around **Result-based workflows** and **domain-oriented error modeling**.

---

## Design Goals

### 1. Native Result Integration

Validation must return:

```csharp
Result
Result<T>
```

No separate validation result type should be required for standard use.

---

### 2. Structured Error Generation

Validation rules must produce:

* `Error.Validation.*`
* `ErrorDetail` entries with:

  * `context` (field name / path)
  * `message`
  * optional metadata

---

### 3. Aggregation by Default

Validation must collect **all failures**, not fail fast.

Example:

```csharp
Result result = validator.Validate(input);

if (!result)
{
    // Contains multiple validation errors
}
```

---

### 4. Fluent API

The API must be:

* readable
* chainable
* predictable

Example:

```csharp
RuleFor(x => x.Email)
    .NotEmpty()
    .Email();

RuleFor(x => x.Password)
    .MinLength(8);
```

---

### 5. Zero Transport Coupling

Validation must NOT:

* know about HTTP
* produce HTTP status codes
* depend on ASP.NET Core

Mapping is handled externally.

---

## Core API

### Validator<T>

```csharp
public abstract class Validator<T>
{
    public Result Validate(T instance);
}
```

---

### RuleFor

```csharp
RuleFor(Expression<Func<T, TProperty>> expression)
```

* Extracts property
* Tracks property name for `ErrorDetail.context`

---

## Built-in Rules (MVP)

### Required

* `NotEmpty()`
* `NotNull()`

### Strings

* `MinLength(int)`
* `MaxLength(int)`
* `Length(int min, int max)`
* `Matches(string pattern)`
* `Email()`

### Numbers

* `GreaterThan`
* `LessThan`
* `InclusiveBetween`

### Generic

* `Must(Func<TProperty, bool>)`

---

## Error Mapping

Each rule must map to a default error:

| Rule     | Error                               |
| -------- | ----------------------------------- |
| NotEmpty | `Error.Validation.MissingField`     |
| Email    | `Error.Validation.InvalidFormat`    |
| Range    | `Error.Validation.ValueOutOfRange`  |
| Custom   | `Error.Validation.InvalidParameter` |

---

## ErrorDetail Generation

Each failure must include:

```json
{
  "context": "email",
  "message": "Email is invalid"
}
```

---

## Customization

### WithMessage

```csharp
RuleFor(x => x.Email)
    .Email()
    .WithMessage("Invalid email format");
```

---

### WithError

```csharp
RuleFor(x => x.Password)
    .MinLength(8)
    .WithError(Error.Validation.ValueOutOfRange("Password too short"));
```

---

## Execution Model

Validation must:

1. Execute all rules
2. Collect all failures
3. Return:

```csharp
Result.Success()
```

or

```csharp
Result.Failure(error)
```

Where `error` may contain multiple `ErrorDetail`

---

## Integration with Results

Validation should support fluent chaining:

```csharp
return validator
    .Validate(request)
    .Then(() => service.Create(request));
```

---

## Future Extensions

### Phase 2

* Nested validators
* Collection validation
* Conditional rules (`When`)
* Rule groups

### Phase 3

* ASP.NET Core auto-validation (optional package)
* Analyzer support
* Source generators (optional)

---

## Non-Goals

* Competing feature-by-feature with FluentValidation
* Supporting every edge-case validation scenario
* Providing transport-specific behavior

---

## Naming

* Package: `LightningArc.Validation`
* Future:

  * `LightningArc.Validation.AspNetCore`

---

## Guiding Principle

> Validation should produce meaningful, structured errors that integrate seamlessly with the LightningArc Result pipeline.

---
