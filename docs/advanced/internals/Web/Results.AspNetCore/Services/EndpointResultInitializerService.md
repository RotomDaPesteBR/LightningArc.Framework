# EndpointResultInitializerService.cs Implementation Details

**File Path:** `Web/Results.AspNetCore/Services/EndpointResultInitializerService.cs`
**Namespace:** `LightningArc.Results.AspNetCore.Services`

## Overview
This is an `IHostedService` implementation. Its sole purpose is to ensure that the mapping services (`SuccessMappingService` and `ErrorMappingService`) are instantiated and initialized when the application starts.

## Code Analysis

### Logic
*   **Constructor Injection**: The service requests `SuccessMappingService` and `ErrorMappingService` in its constructor.
*   **Effect**: Since these services are Singletons, requesting them forces the DI container to create them immediately. This ensures that any initialization logic within those services (like logging configured mappings or validating configuration) happens at startup, rather than lazily on the first request.
*   **StartAsync/StopAsync**: These methods are empty because the "work" is done during instantiation.
