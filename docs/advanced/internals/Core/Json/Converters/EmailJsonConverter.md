# EmailJsonConverter.cs Implementation Details

**File Path:** `Core/Json/Converters/EmailJsonConverter.cs`
**Namespace:** `LightningArc.Json.Converters`

## Overview
The `EmailJsonConverter` class is a custom JSON converter for the `System.Text.Json` library. It handles the serialization and deserialization of the `Email` Value Object (from `Abstractions`).

## Code Analysis

### Inheritance
It inherits from `JsonConverter<Email>`, specializing the conversion logic for the `Email` type.

### Deserialization (`Read`)
```csharp
public override Email Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
```
1.  **Null Check**: Throws a `JsonException` if the JSON token is `null`. The domain requires a valid email.
2.  **Type Check**: Verifies that the JSON token is a `String`. If not, throws a `JsonException`.
3.  **Creation**: Reads the string value and attempts to create an `Email` object using `Email.Create()`.
    *   **Error Handling**: If `Email.Create()` throws an `ArgumentException` (due to invalid format), it is caught and wrapped in a `JsonException`. This ensures that deserialization failures are reported as JSON errors.

### Serialization (`Write`)
```csharp
public override void Write(Utf8JsonWriter writer, Email value, JsonSerializerOptions options)
```
*   **Logic**: Simply calls `writer.WriteStringValue(value.Value)`.
*   **Result**: The `Email` object is serialized as a simple JSON string (e.g., `"user@example.com"`) rather than a complex object (e.g., `{"Value": "user@example.com"}`). This provides a cleaner API contract.
