# LightningArc.Results assembly

## LightningArc.Results namespace

| public type | description |
| --- | --- |
| class [AggregateError](./LightningArc.Results/AggregateError.md) | Represents a group of one or more errors that occurred during an operation. |
| struct [AsyncCheck](./LightningArc.Results/AsyncCheck.md) | Pairs an async check with an optional callback invoked with that specific check's [`Result`](./LightningArc.Results/Result.md) the moment it resolves — independent of when the rest of the batch completes. |
| class [Error](./LightningArc.Results/Error.md) | Represents a standardized error object for the application. |
| struct [ErrorDetail](./LightningArc.Results/ErrorDetail.md) | Represents a specific detail of an error, providing additional context and a descriptive message. |
| class [Result&lt;TValue&gt;](./LightningArc.Results/Result-1.md) | Represents the result of an operation that can be a success (with a specific value) or a failure. |
| class [Result](./LightningArc.Results/Result.md) | Represents the result of an operation that can be either a success or a failure. This is the base class for the Result pattern, used when an operation does not return a specific value upon success. |
| class [ResultAccessFailedException](./LightningArc.Results/ResultAccessFailedException.md) | Exception thrown when an attempt to access the value or error of a [`Result`](./LightningArc.Results/Result.md) is made in an invalid way (e.g., accessing the value of a failure [`Result`](./LightningArc.Results/Result.md)). |
| class [ResultAggregator](./LightningArc.Results/ResultAggregator.md) | Provides a fluent builder for aggregating multiple validation results without short-circuiting. |
| static class [ResultAggregatorExtensions](./LightningArc.Results/ResultAggregatorExtensions.md) | Provides extension methods for [`ResultAggregator`](./LightningArc.Results/ResultAggregator.md) that mirror the instance methods for fluent chaining. |
| static class [ResultAggregatorTaskExtensions](./LightningArc.Results/ResultAggregatorTaskExtensions.md) | Provides extension methods for Task that mirror the instance methods of [`ResultAggregator`](./LightningArc.Results/ResultAggregator.md) for fluent async chaining. |
| static class [ResultExtensions](./LightningArc.Results/ResultExtensions.md) | Provides extension methods for the [`Result`](./LightningArc.Results/Result-1.md) class, enabling the composition of functional operations. |
| abstract class [Success&lt;TValue&gt;](./LightningArc.Results/Success-1.md) | Represents a success result that encapsulates a specific value along with success metadata. This class extends [`Success`](./LightningArc.Results/Success.md) to provide a typed value. |
| abstract class [Success](./LightningArc.Results/Success.md) | Represents a generic success result. This base class is used to standardize success responses, allowing subclasses to define specific types of success. |
| static class [SuccessExtensions](./LightningArc.Results/SuccessExtensions.md) | Provides extension methods for the [`Success`](./LightningArc.Results/Success-1.md) class, enabling fluent value transformation (mapping). |
| struct [WhenCondition](./LightningArc.Results/WhenCondition.md) | Represents a condition paired with its error for use with [`WhenAll`](./LightningArc.Results/ResultAggregatorExtensions/WhenAll.md). |

## LightningArc.Results.Errors namespace

| public type | description |
| --- | --- |
| class [ErrorInformation](./LightningArc.Results.Errors/ErrorInformation.md) | Class to encapsulate the details of a specific error. |

## LightningArc.Results.Exceptions namespace

| public type | description |
| --- | --- |
| static class [ExceptionExtensions](./LightningArc.Results.Exceptions/ExceptionExtensions.md) | Provides extension methods for converting exceptions to [`Error`](./LightningArc.Results/Error.md) objects. |
| static class [ExceptionMapper](./LightningArc.Results.Exceptions/ExceptionMapper.md) | Provides a centralized registry and transformer for mapping exceptions to [`Error`](./LightningArc.Results/Error.md) objects. |

## LightningArc.Results.Localization namespace

| public type | description |
| --- | --- |
| static class [LocalizationManager](./LightningArc.Results.Localization/LocalizationManager.md) | Manages localization for error messages within the library. This class can be optionally configured by the consumer to set a specific culture and resource manager for error message lookup. |

## LightningArc.Results.Messages namespace

| public type | description |
| --- | --- |
| interface [IMessageProvider](./LightningArc.Results.Messages/IMessageProvider.md) | Defines a contract for message providers, allowing for different strategies to obtain the message of an error or success. |
| class [LiteralMessageProvider](./LightningArc.Results.Messages/LiteralMessageProvider.md) | Message provider for literal (static) messages. |
| class [ResourceMessageProvider](./LightningArc.Results.Messages/ResourceMessageProvider.md) | Message provider for localized messages via resource key. |

## LightningArc.Results.Successes namespace

| public type | description |
| --- | --- |
| class [AcceptedSuccess&lt;TValue&gt;](./LightningArc.Results.Successes/AcceptedSuccess-1.md) | Represents the success type for an "Accepted" operation with a *TValue* value. |
| class [AcceptedSuccess](./LightningArc.Results.Successes/AcceptedSuccess.md) | Represents the success type for an "Accepted" operation. |
| class [CreatedSuccess&lt;TValue&gt;](./LightningArc.Results.Successes/CreatedSuccess-1.md) | Represents the success type for a "Created" operation with a *TValue* value. |
| class [CreatedSuccess](./LightningArc.Results.Successes/CreatedSuccess.md) | Represents the success type for a "Created" operation. |
| class [NoContentSuccess&lt;TValue&gt;](./LightningArc.Results.Successes/NoContentSuccess-1.md) | Represents the success type for a "No Content" operation with a *TValue* value. |
| class [NoContentSuccess](./LightningArc.Results.Successes/NoContentSuccess.md) | Represents the success type for a "No Content" operation. |
| class [OkSuccess&lt;TValue&gt;](./LightningArc.Results.Successes/OkSuccess-1.md) | Represents the success type for an "Ok" operation with a *TValue* value. |
| class [OkSuccess](./LightningArc.Results.Successes/OkSuccess.md) | Represents the success type for an "Ok" operation. |

<!-- DO NOT EDIT: generated by xmldocmd for LightningArc.Results.dll -->
