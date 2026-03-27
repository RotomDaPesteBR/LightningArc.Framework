# DependencyInjection.cs Implementation Details

**File Path:** `Web/Results.AspNetCore/DependencyInjection.cs`
**Namespace:** `LightningArc.AspNetCoreCoreCoreCore`

## Overview
This file contains the `ServiceCollectionExtensions` and `WebApplicationExtensions` classes, responsible for configuring and registering the library's services in the ASP.NET Core Dependency Injection container.

## Code Analysis

### `AddEndpointResults`
```csharp
public static IServiceCollection AddEndpointResults(...)
```
*   **Purpose**: Main entry point for configuration.
*   **Parameters**:
    *   `wrapSuccessResponses`: Option to wrap JSON responses in a standard envelope.
    *   `configureMappings`: Action to define custom mappings between Domain Errors/Successes and HTTP Status Codes.
    *   `defaultCulture`: Sets the default culture for error messages.
*   **Registration**:
    *   Registers `EndpointResultOptions` as a Singleton.
    *   Registers `SuccessMappingService` and `ErrorMappingService` (Singletons).
    *   Registers `IErrorListProvider` (for documentation generation).
    *   Registers `EndpointResultInitializerService` as a Hosted Service.

### `OutputErrorsList`
```csharp
public static void OutputErrorsList(this WebApplication app, ...)
```
*   **Purpose**: Generates a Markdown file documenting all registered errors in the system.
*   **Usage**: Intended for development/build pipelines to keep API documentation in sync with the code.
*   **Mechanism**: Resolves `IErrorListProvider` from the DI container and writes the formatted output to disk.


