# LightningArc.Results.AspNetCore assembly

## LightningArc.Results.AspNetCore namespace

| public type | description |
| --- | --- |
| class [CustomErrorMapping](./LightningArc.Results.AspNetCore/CustomErrorMapping.md) | Represents a custom error mapping configuration. |
| class [CustomSuccessMapping](./LightningArc.Results.AspNetCore/CustomSuccessMapping.md) | Represents a custom success mapping configuration. |
| class [EndpointResult&lt;TValue&gt;](./LightningArc.Results.AspNetCore/EndpointResult-1.md) | Represents a generic endpoint result that maps a Result to an appropriate HTTP response, including the status code and the body. |
| class [EndpointResult](./LightningArc.Results.AspNetCore/EndpointResult.md) | Represents an endpoint result that maps a Result to an appropriate HTTP response. |
| class [EndpointResultOptions](./LightningArc.Results.AspNetCore/EndpointResultOptions.md) | Options class for configuring success and error mappings. |
| class [ErrorMapping](./LightningArc.Results.AspNetCore/ErrorMapping.md) | Class to store the details of mapping an error to an HTTP response. |
| class [ErrorMappingConfigurator](./LightningArc.Results.AspNetCore/ErrorMappingConfigurator.md) | Provides a fluent API for configuring custom error mappings. |
| class [ErrorMappingService](./LightningArc.Results.AspNetCore/ErrorMappingService.md) | Service to map specific error types (Error) to appropriate HTTP responses (status code and problem details). |
| static class [ErrorMappingServiceExtensions](./LightningArc.Results.AspNetCore/ErrorMappingServiceExtensions.md) | Provides extension methods for the [`ErrorMappingService`](./LightningArc.Results.AspNetCore/ErrorMappingService.md) to group and add error mappings in a modular way. |
| class [ErrorMetadata](./LightningArc.Results.AspNetCore/ErrorMetadata.md) | Represents the structured metadata of a registered error. |
| class [ErrorResult](./LightningArc.Results.AspNetCore/ErrorResult.md) | Represents an HTTP result that maps an Error instance to a standard "Problem Details" response (RFC 7807). |
| class [ResultExceptionHandler](./LightningArc.Results.AspNetCore/ResultExceptionHandler.md) | Handles global exceptions and converts them into standardized Error responses consistent with the library's [`EndpointResult`](./LightningArc.Results.AspNetCore/EndpointResult.md) pattern. |
| static class [ResultExtensions](./LightningArc.Results.AspNetCore/ResultExtensions.md) | Provides extension methods for the Result class, allowing configuration of the content type (Content-Type) for the HTTP response. |
| static class [ServiceCollectionExtensions](./LightningArc.Results.AspNetCore/ServiceCollectionExtensions.md) | Provides extension methods for registering the endpoint results feature in the application's service collection. |
| class [SuccessDetail](./LightningArc.Results.AspNetCore/SuccessDetail.md) | Represents detailed information about a successful operation, used for custom success response formatting. |
| class [SuccessMapping](./LightningArc.Results.AspNetCore/SuccessMapping.md) | Represents mapping information for a success type. |
| class [SuccessMappingConfigurator](./LightningArc.Results.AspNetCore/SuccessMappingConfigurator.md) | Provides a fluent API for configuring custom success mappings. |
| class [SuccessMappingService](./LightningArc.Results.AspNetCore/SuccessMappingService.md) | Service to map specific success types (Success) to appropriate HTTP responses (status code and problem details). |
| class [SuccessResult&lt;TValue&gt;](./LightningArc.Results.AspNetCore/SuccessResult-1.md) | Represents an HTTP result that maps a Success instance and a value to a standard success response. |
| class [SuccessResult](./LightningArc.Results.AspNetCore/SuccessResult.md) | Represents an HTTP result that maps a Success instance to a standard success response. |
| static class [WebApplicationExtensions](./LightningArc.Results.AspNetCore/WebApplicationExtensions.md) | Provides extension methods for configuring the ASP.NET Core request pipeline. |

## LightningArc.Results.AspNetCore.Interfaces namespace

| public type | description |
| --- | --- |
| interface [IErrorListFormatter](./LightningArc.Results.AspNetCore.Interfaces/IErrorListFormatter.md) | Defines a contract for a service that formats a list of error metadata into a specific string representation. |
| interface [IErrorListProvider](./LightningArc.Results.AspNetCore.Interfaces/IErrorListProvider.md) | Defines a contract for a service that provides a list of all registered error metadata. |

## LightningArc.Results.AspNetCore.Localization namespace

| public type | description |
| --- | --- |
| static class [LocalizationManager](./LightningArc.Results.AspNetCore.Localization/LocalizationManager.md) | Manages localization for error titles within the ASP.NET Results library. This class can be optionally configured by the consumer to set a specific culture and resource manager for error title lookup. |

<!-- DO NOT EDIT: generated by xmldocmd for LightningArc.Results.AspNetCore.dll -->
