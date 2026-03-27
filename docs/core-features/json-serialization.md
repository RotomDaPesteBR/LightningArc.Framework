# Json Usage Guide

The **Json** library provides integration with `System.Text.Json` for the Value Objects and types defined in the LightningArc ecosystem.

## Features

*   **Custom Converters**: Enables seamless serialization of complex Value Objects (like `Email`) into simple JSON types (like strings).

## Setup

To use the custom converters, you need to add them to your `JsonSerializerOptions`.

### In ASP.NET Core
Configure the JSON options in your `Program.cs` or `Startup.cs`:

```csharp
using LightningArc.Json.Converters;

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Extension method to add all Utils converters
        options.JsonSerializerOptions.Converters.AddJsonConverters();
    });
```

### Manual Serialization
If you are manually using `JsonSerializer`:

```csharp
using System.Text.Json;
using LightningArc.Json.Converters;

var options = new JsonSerializerOptions();
options.Converters.AddJsonConverters();

var myData = new MyDto { Email = Email.Create("test@test.com") };

// Serializes Email as a string: {"Email": "test@test.com"}
string json = JsonSerializer.Serialize(myData, options);
```

## Supported Types

| Type | JSON Representation |
| :--- | :--- |
| `Email` | String (`"user@example.com"`) |

Without these converters, the `Email` object might be serialized as a complex object `{"Value": "user@example.com"}`, which exposes implementation details and complicates the API contract.

---

## Implementation Details

For deep dives into the source code, see:
*   [Json Implementation](../advanced/internals/Core/Json/README.md)
*   [EmailJsonConverter.cs](../advanced/internals/Core/Json/Converters/EmailJsonConverter.md)