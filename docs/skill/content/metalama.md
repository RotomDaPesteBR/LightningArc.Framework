---
uid: metalama
level: 200
summary: "Guide on how LightningArc.Utils extends Metalama to simplify the creation of aspects and automate code generation."
keywords: "Metalama, AOP, Aspects, Code Generation, Boilerplate, AddMethod, AddAsyncMethod"
---

# Metalama Extensions

`LightningArc.Metalama` provides advanced extensions for the Metalama framework, specifically designed to simplify the creation of aspects that introduce new members to types.

## IAspectBuilder Extensions

The `IAspectBuilderExtensions` class offers a fluent API to add methods to target types during compilation.

### Key Methods

*   **AddMethod**: Introduces a synchronous method. Handles boilerplate configuration of the method builder.
*   **AddAsyncMethod**: Introduces an asynchronous method. It automatically appends "Async" to the name and wraps the return type in `Task<T>` or `ValueTask<T>`.

### Usage Example (Within an Aspect)

```csharp
[CompileTime]
public override void BuildAspect(IAspectBuilder<INamedType> builder)
{
    // Adding a standard Sync method
    builder.AddMethod(
        templateName: nameof(this.MySyncTemplate),
        methodName: "Process",
        returnType: typeof(void).ToIType()
    );

    // Adding an Async method with automatic Task wrapping
    builder.AddAsyncMethod(
        templateName: nameof(this.MyAsyncTemplate),
        methodName: "Process",
        returnType: typeof(bool).ToIType(),
        asyncType: AsyncType.ValueTask
    );
}
```

## Type Factories

The `Metalama.Factories` namespace includes utilities to retrieve `IType` instances for common wrappers:
*   `TaskTypeFactory`: Get `Task`, `Task<T>`, `ValueTask<T>`.
*   `CollectionTypeFactory`: Get `IEnumerable<T>`, `List<T>`, etc.
*   `ResultTypeFactory`: Get `Result<T>` types for use in generated code.

## Purpose
These tools are intended for developers building high-level frameworks or internal tools that require consistent code generation patterns across multiple projects.
