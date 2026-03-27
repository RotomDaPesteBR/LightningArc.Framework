# Metalama Usage Guide

The **Metalama** libraries provide the infrastructure for building custom Aspects using the Metalama framework within the LightningArc ecosystem.

## Overview

These libraries are **Meta-libraries**, meaning they are used to build *other* libraries or tools. They run primarily at **Compile Time**.

*   **Extensions**: Helpers to simplify the Metalama API (e.g., `AddAsyncMethod`).
*   **Factories**: Helpers to obtain `IType` references for common types (`Task`, `Result`) without manually looking them up by string names.

## Usage Example (Creating an Aspect)

If you were building an aspect to automatically wrap a method execution in a `Result`, you would use these tools:

```csharp
[CompileTime]
public override void BuildAspect(IAspectBuilder<INamedType> builder)
{
    // Use Factory to get the return type "Task<Result<int>>"
    var returnType = ResultTypeFactory.GetTaskType(typeof(int));

    // Use Extension to introduce the method easily
    builder.AddAsyncMethod(
        templateName: nameof(MyTemplate),
        methodName: "MyGeneratedMethod",
        returnType: returnType
    );
}
```

## Dependencies
These projects depend on `Metalama.Framework`. Ensure you have a valid Metalama license or trial to build projects that use these aspects.

---

## Implementation Details

For deep dives into the source code, see:
*   [Metalama.Extensions Implementation](../advanced/internals/Meta/Utils.Metalama.Extensions/README.md)
*   [Metalama.Factories Implementation](../advanced/internals/Meta/Utils.Metalama.Factories/README.md)