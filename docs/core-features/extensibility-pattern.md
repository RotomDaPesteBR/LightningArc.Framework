# Extensibility Pattern in LightningArc.Results

The LightningArc.Results library provides an extensibility pattern that allows developers to create domain-specific extensions for both success and error handling while maintaining a clean separation of concerns.

## Overview

The extensibility pattern introduces two key entry points:
- `Result.Of` - Provides access to success creation hooks
- `Success.Of` - Provides access to success factory methods  
- `Error.Of<TModule>()` - Provides access to error module creation (existing pattern)

These entry points return specialized "Hook" classes that act as fluent interfaces for creating domain-specific extensions.

## Access Patterns

### Modern C# / .NET 10+ (Extension Members)

The preferred approach uses static extension members (available in .NET 10+):

```csharp
extension(Result)
{
    public static Result OrderConfirmed()
    {
        return new OrderConfirmedSuccess();
    }
}

extension(Error)
{
    public static Error.ErrorModule<Ordering> Ordering => Error.Of<Ordering>();
}
```

**Usage:**
- `Result.OrderConfirmed()` (accessed via extension member on `Result`)
- `Error.Ordering.InventoryInsufficient()` (accessed via extension member on `Error`)

### Compatibility Surface (Traditional Extension Methods)

For projects targeting earlier .NET versions, use traditional static extension methods with the `Of` property:

```csharp
public static class ResultExtensions
{
    public static Result OrderConfirmed(this Result result)
    {
        return new OrderConfirmedSuccess();
    }
}

public static class ErrorExtensions
{
    public static Error.ErrorModule<Ordering> Ordering(this Error error) => Error.Of<Ordering>();
}
```

**Usage:**
- `Result.Of.OrderConfirmed()` (accessed via `Result.Of`)
- `Error.Of<Ordering>().InventoryInsufficient()` (accessed via `Error.Of`)

> **Important**: `Of` is the compatibility entry point for the same extension model on targets where static extension members are unavailable. Both approaches provide identical functionality through different syntax.

## Core Components

### Result.Of Property

```csharp
/// <summary>
/// Provides a fluent entry point to create custom success
/// through the <see cref="Success.Hook"/> mechanism.
/// </summary>
public static Success.Hook Of => Results.Success.Of;
```

The `Result.Of` property returns a `Success.Hook` instance that provides access to success creation functionality.

### Success.Of Property

```csharp
/// <summary>
/// Provides a fluent entry point to create custom success
/// through the <see cref="Hook"/> mechanism.
/// </summary>
public static Hook Of => new();
```

The `Success.Of` property returns a `Hook` instance that provides access to success factory methods.

### Hook Class

```csharp
/// <summary>
/// Provides access to success-related factory methods and extension methods.
/// </summary>
public sealed class Hook
{
    internal Hook() { }
}
```

The `Hook` class is an internal constructor, sealed class that serves as the base for all extension hooks. Its internal constructor ensures that only the library can create instances, while the sealed modifier prevents inheritance.

## Creating Domain-Specific Extensions

To create domain-specific extensions, follow this pattern:

1. Create a class that inherits from `Error.ErrorModule` (for errors) or `Success`/`Success<TValue>` (for successes)
2. Create extension methods that operate on the appropriate Hook type
3. Use the `Of` properties to access your extensions

### Example: Order Processing Domain

```csharp
// Define a domain-specific error module
public class Ordering : Error.ErrorModule
{
    public new const int CodePrefix = 12;

    // Specific error for this domain
    public class InventoryInsufficientError : Error
    {
        internal InventoryInsufficientError(string message, List<ErrorDetail>? details = null)
            : base(Ordering.CodePrefix, 01, message, details) { }
    }
}

// Extension methods for creating domain-specific errors
public static class OrderingErrorExtensions
{
    // Extension that provides access to the ordering error module
    extension(Error)
    {
        public static Error.ErrorModule<Ordering> Ordering => Error.Of<Ordering>();
    }

    // Extension method for creating a specific domain error
    public static Error InventoryInsufficient(
        this Error.ErrorModule<Ordering> _,
        string message = "Insufficient inventory to fulfill order",
        List<ErrorDetail>? details = null
    ) => new InventoryInsufficientError(message, details);
}

// Extension method for creating domain-specific successes
public static partial class ResultExtensions
{
    extension(Result)
    {
        public static Result OrderConfirmed()
        {
            return new OrderConfirmedSuccess();
        }
    }
}

// Success implementation for the domain
public class OrderConfirmedSuccess : Success
{
    internal OrderConfirmedSuccess() : base(101, "Order confirmed") { }
    
    internal OrderConfirmedSuccess(string? message) : base(101, message) { }
}

// Extension method accessed via Success.Of
public static partial class OrderingSuccessExtensions
{
    public static Result OrderConfirmed(this Success.Hook _)
    {
        return new OrderConfirmedSuccess();
    }
}
```

## Usage Examples

### Accessing Domain Extensions

```csharp
// Creating a domain-specific error (via Error.Of)
var orderingError = Error.Of<Ordering>().InventoryInsufficient();

// Creating a domain-specific success via Result.Of
var orderingSuccess = Result.Of.OrderConfirmed();

// Creating a domain-specific success via Success.Of
var orderingSuccessAlt = Success.Of.OrderConfirmed();

// Using in a controller context
return Error.Of<Ordering>().InventoryInsufficient("Low stock for product SKU-123", 
    [new ErrorDetail("ProductId", "SKU-123")]);
```

## Recommended Approach: Extension Blocks (.NET 10+)

The preferred method for implementing extensions uses extension blocks (available in .NET 10+):

```csharp
extension(Result)
{
    public static Result OrderConfirmed()
    {
        return new OrderConfirmedSuccess();
    }
}

extension(Error)
{
    public static Error.ErrorModule<Ordering> Ordering => Error.Of<Ordering>();
}
```

**Usage with extension blocks:**
- `Result.OrderConfirmed()` (accessed directly on `Result`)
- `Error.Ordering.InventoryInsufficient()` (accessed directly on `Error`)

This approach provides:
- Clean, localized extension method definitions
- No need for separate static classes
- Natural grouping of related extensions
- Full compatibility with the `Of` property access pattern

### Compatibility Approach: Traditional Extension Methods

For projects targeting earlier .NET versions, use traditional static extension methods:

```csharp
public static class ResultExtensions
{
    public static Result OrderConfirmed(this Result result)
    {
        return new OrderConfirmedSuccess();
    }
}

public static class ErrorExtensions
{
    public static Error.ErrorModule<Ordering> Ordering(this Error error) => Error.Of<Ordering>();
}
```

**Usage with traditional extensions:**
- `Result.Of.OrderConfirmed()` (accessed via `Result.Of`)
- `Error.Of<Ordering>().InventoryInsufficient()` (accessed via `Error.Of`)

Both approaches produce identical functionality:
- Extension members (modern): `Result.OrderConfirmed()`, `Error.Ordering.InventoryInsufficient()`
- Traditional extensions (compatibility): `Result.Of.OrderConfirmed()`, `Error.Of<Ordering>().InventoryInsufficient()`

## Best Practices

1. **Domain Separation**: Create separate modules for different business domains (e.g., `Ordering`, `Inventory`, `Shipping`)
2. **Clear Naming**: Use descriptive names for extension methods that clearly indicate their purpose
3. **Consistent Pattern**: Follow the established pattern of `Result.Of` for success extensions and `Error.Of<TModule>()` for error modules
4. **Internal Constructors**: Keep success/error class constructors internal to enforce factory method usage
5. **Documentation**: Provide XML documentation for all public extension methods and classes
6. **Localization**: For production code, consider localizing messages through `IMessageProvider` rather than hardcoding strings
7. **Testing**: Create unit tests for your custom extensions to ensure they behave as expected

## Benefits of This Pattern

1. **Discoverability**: Extensions are easily discoverable through IntelliSense via the `Of` properties
2. **Type Safety**: The pattern provides compile-time safety for extension method usage
3. **Separation of Concerns**: Domain-specific extensions don't pollute the core Result/Success/Error APIs
4. **Fluent Interface**: Enables fluent, readable code for creating domain-specific results
5. **Extensibility**: Easy to add new extensions without modifying existing code
6. **Backward Compatibility**: The pattern is additive and doesn't break existing usage

## Related Documentation

- [Result Pattern Documentation](../result-pattern.md)
- [Error Implementation Details](../Results/Error.md)
- [Success Implementation Details](../Results/Success.md)
- [Result Implementation Details](../Results/Result.md)