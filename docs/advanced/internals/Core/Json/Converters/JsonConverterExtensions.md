# JsonConverterExtensions.cs Implementation Details

**File Path:** `Core/Json/Converters/JsonConverterExtensions.cs`
**Namespace:** `LightningArc.Json.Converters`

## Overview
This class provides extension methods to simplify the registration of custom JSON converters defined in this library.

## Code Analysis

### `AddJsonConverters`
```csharp
public static ICollection<JsonConverter> AddJsonConverters(this ICollection<JsonConverter> converters)
```
*   **Target**: Extends `ICollection<JsonConverter>`, which is the type of the `JsonSerializerOptions.Converters` list.
*   **Functionality**: Adds the `EmailJsonConverter` to the list.
*   **Return**: Returns the modified collection to allow for method chaining (fluent interface), although `ICollection` usually implies void modification, this pattern is common in configuration builders.
*   **Extensibility**: As more Value Objects and converters are added to the library, they should be registered here to provide a single point of configuration for the consumer.
