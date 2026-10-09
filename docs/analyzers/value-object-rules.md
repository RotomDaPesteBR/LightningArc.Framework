# ValueObject Rules

Rules LARC020–LARC023 enforce explicit handling of ValueObject creation and string conversions, plus record shape. Each section below summarizes the rule — follow the link for the full description, examples, suppression, and limitations.

---

## LARC020

**Implicit conversion from `string` to ValueObject** — Warning — Code fix: yes

A string literal implicitly converted to a ValueObject (`Email`, `Cpf`, `Cnpj`, `PhoneNumber`, `Url`, or any `IValueObject`/`IValueObject<T>`) in a variable declarator, an assignment, or a call argument. The implicit conversion can throw on invalid input; prefer explicit `Create`/`TryCreate` validation. The fix wraps the literal as `<VO>.Create("literal")`.

Details: [LARC020](LARC020.md)

---

## LARC021

**Potential null ValueObject conversion to `string`** — Warning — Code fix: yes

A nullable-annotated ValueObject used where a `string` is expected — in a `string` variable declarator, a `string` assignment target, or a `string` call parameter. The implicit conversion to string can throw on null. The fix rewrites the expression to `expr?.Value ?? string.Empty`.

Details: [LARC021](LARC021.md)

---

## LARC022

**ValueObject creation result is discarded** — Info — Code fix: yes

A `Create(...)`/`TryCreate(...)` call on a ValueObject type used as a bare statement. The validated value (or validation failure) is thrown away, so the validation never takes effect. The fix prefixes the statement with `_ = ` when the discard is deliberate.

Details: [LARC022](LARC022.md)

---

## LARC023

**ValueObject should be a record** — Warning — Code fix: yes

A ValueObject (a type implementing `IValueObject`/`IValueObject<T>` or inheriting a known ValueObject type) declared as a `class`. ValueObjects need value-based equality, which records provide by default. The fix converts the `class` to a `record` — review the type for hand-written equality members before applying it.

Details: [LARC023](LARC023.md)
