# IpAddress

**Namespace:** `LightningArc.Primitives.ValueObjects`
**Type:** `record`

Represents a validated IP address (IPv4 or IPv6). This type ensures immutability and identifies the IP version at creation time.

---

## Properties

### Value
- **Type:** `string`
- **Description:** The IP address string representation.

### Version
- **Type:** `int`
- **Description:** The IP version (`4` for IPv4, `6` for IPv6).

---

## Methods

### Create(string value)
- **Static:** Yes
- **Description:** Validates the input string as a well-formed IP address and creates a new instance.
- **Parameters:**
  - `value` (`string`): The IP address string.
- **Returns:** A new `IpAddress` instance.
- **Exceptions:**
  - `ArgumentException`: Thrown if the value is null, empty, or not a valid IP address.

### TryCreate(string value, out IpAddress? result)
- **Static:** Yes
- **Description:** Attempts to create an `IpAddress` without throwing exceptions.
- **Parameters:**
  - `value` (`string`): The IP address string.
  - `result` (`out IpAddress?`): The created instance if valid, or `null`.
- **Returns:** `bool` — `true` if the IP address is valid; otherwise, `false`.

### ToString()
- **Description:** Returns the IP address string.
- **Returns:** `string`

---

## Operators

### implicit operator string(IpAddress ipAddress)
- **Description:** Allows implicit conversion from an `IpAddress` object to a `string`.

### implicit operator IpAddress(string value)
- **Description:** Allows implicit conversion from a `string` to an `IpAddress` object, triggering validation.

---

## Usage Examples

```csharp
using LightningArc.Primitives.ValueObjects;

// Direct creation
var ipv4 = IpAddress.Create("192.168.1.1");
Console.WriteLine(ipv4.Version); // 4

var ipv6 = IpAddress.Create("::1");
Console.WriteLine(ipv6.Version); // 6

// TryCreate for safe handling
if (IpAddress.TryCreate("10.0.0.1", out var validIp))
{
    // validIp is guaranteed valid
}

// AsIpAddress for fluent handling
Result<IpAddress> result = "invalid".AsIpAddress();

// Implicit conversion to Result<IpAddress>
Result<IpAddress> wrapped = ip;
```
