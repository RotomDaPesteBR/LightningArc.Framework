# Metalama AOP Integration

The **`LightningArc.Metalama`** module provides advanced utilities to build compile-time aspects that integrate seamlessly with the `Result` pattern.

## Core Utilities for Aspect Authors

### `NamedTypeFactory`
Used to resolve core library types during the compilation phase.
- `NamedTypeFactory.Result(context)`: Resolves the `Result` type.
- `NamedTypeFactory.TaskResult(context)`: Resolves the `TaskResult` type.

### `ResultTypeExtensions`
Simplifies checking and manipulating return types in aspects.
```csharp
public override void BuildAspect(IAspectBuilder<IMethod> builder)
{
    var returnType = builder.Target.ReturnType;
    
    if (returnType.IsResult()) { /* Logic for Result */ }
    if (returnType.IsTaskResult()) { /* Logic for TaskResult */ }
}
```

## Example: Result-Aware Logging Aspect
This pattern allows an aspect to automatically wrap a successful return or capture a failure without manual `IsSuccess` checks.

```csharp
[Template]
public dynamic OverrideMethod()
{
    var result = meta.Proceed();
    
    if (result is Result res && !res)
    {
        // Aspect logic for failure
        Logger.LogError(res.Error.Message);
    }
    
    return result;
}
```

## Compilation Metadata
The project uses `MetalamaCompileTimeAssembly` to ensure that utilities are available during the compilation of dependent projects.
