# Currency

**Namespace:** `LightningArc.Primitives.ValueObjects`
**Type:** `record`

Represents a monetary value with a validated ISO 4217 currency code. This type ensures immutability and guarantees that the currency code is a recognized 3-letter code at the moment of creation.

---

## Properties

### Value
- **Type:** `decimal`
- **Description:** The monetary amount.

### Code
- **Type:** `string`
- **Description:** The ISO 4217 currency code (e.g., `BRL`, `USD`, `EUR`), normalized to uppercase.

---

## Methods

### Create(decimal value, string code)
- **Static:** Yes
- **Description:** Validates the currency code against known ISO 4217 codes and creates a new instance.
- **Parameters:**
  - `value` (`decimal`): The monetary amount.
  - `code` (`string`): The 3-letter ISO 4217 currency code.
- **Returns:** A new `Currency` instance.
- **Exceptions:**
  - `ArgumentException`: Thrown if the code is null, empty, or not a recognized ISO 4217 code.

### TryCreate(decimal value, string code, out Currency? result)
- **Static:** Yes
- **Description:** Attempts to create a `Currency` without throwing exceptions.
- **Parameters:**
  - `value` (`decimal`): The monetary amount.
  - `code` (`string`): The 3-letter ISO 4217 currency code.
  - `result` (`out Currency?`): The created instance if valid, or `null`.
- **Returns:** `bool` — `true` if the currency code is valid; otherwise, `false`.

### ToString()
- **Description:** Returns the monetary value with its currency code (e.g., `1234.56 BRL`).
- **Returns:** `string`

---

## Operators

### implicit operator decimal(Currency currency)
- **Description:** Allows implicit conversion from a `Currency` object to its `decimal` value.

---

## Usage Examples

```csharp
using LightningArc.Primitives.ValueObjects;

// Direct creation
var price = Currency.Create(99.90m, "BRL");
var usd = Currency.Create(19.99m, "USD");

// TryCreate for safe handling
if (Currency.TryCreate(50.00m, "EUR", out var validCurrency))
{
    // validCurrency is guaranteed valid
}

// AsCurrency for fluent handling
Result<Currency> result = 100.00m.AsCurrency();

// Implicit conversion to Result<Currency>
Result<Currency> wrapped = currency;
```
// Implicit conversion to decimal
decimal amount = price; // 99.90
```
