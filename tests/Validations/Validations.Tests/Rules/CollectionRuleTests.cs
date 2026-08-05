using LightningArc.Results;
using LightningArc.Validations;

namespace LightningArc.Validations.Tests.Rules;

public sealed record OrderLineCommand(string Sku);
public sealed record OrderCommand(IReadOnlyList<OrderLineCommand> Items);

public sealed class OrderLineValidator : AbstractValidator<OrderLineCommand>
{
    public OrderLineValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty();
    }
}

public sealed class OrderValidator : AbstractValidator<OrderCommand>
{
    public OrderValidator()
    {
        RuleForEach(x => x.Items)
            .SetValidator(new OrderLineValidator());
    }
}

public sealed class DetaillessOrderLineValidator : IValidator<OrderLineCommand>
{
    public Result Validate(OrderLineCommand instance) => Error.Application.InvalidOperation("Collection child validator failed without details");
    public Result Validate(OrderLineCommand instance, string ruleSet) => Validate(instance);
    public Result Validate(OrderLineCommand instance, ValidationOptions options) => Validate(instance);
    public Task<Result> ValidateAsync(OrderLineCommand instance, CancellationToken cancellationToken = default) => Task.FromResult(Validate(instance));
    public Task<Result> ValidateAsync(OrderLineCommand instance, string ruleSet, CancellationToken cancellationToken = default) => Task.FromResult(Validate(instance));
    public Task<Result> ValidateAsync(OrderLineCommand instance, ValidationOptions options) => Task.FromResult(Validate(instance));
}

public sealed class ResourceOrderLineValidator : IValidator<OrderLineCommand>
{
    public Result Validate(OrderLineCommand instance) => Error.Resource.NotFound("Order line resource was not found");
    public Result Validate(OrderLineCommand instance, string ruleSet) => Validate(instance);
    public Result Validate(OrderLineCommand instance, ValidationOptions options) => Validate(instance);
    public Task<Result> ValidateAsync(OrderLineCommand instance, CancellationToken cancellationToken = default) => Task.FromResult(Validate(instance));
    public Task<Result> ValidateAsync(OrderLineCommand instance, string ruleSet, CancellationToken cancellationToken = default) => Task.FromResult(Validate(instance));
    public Task<Result> ValidateAsync(OrderLineCommand instance, ValidationOptions options) => Task.FromResult(Validate(instance));
}

public sealed class DetailedResourceOrderLineValidator : IValidator<OrderLineCommand>
{
    public Result Validate(OrderLineCommand instance) => Error.Resource.NotFound(
        "Order line resource was not found",
        [new ErrorDetail(nameof(OrderLineCommand.Sku), "Sku could not be resolved")]);
    public Result Validate(OrderLineCommand instance, string ruleSet) => Validate(instance);
    public Result Validate(OrderLineCommand instance, ValidationOptions options) => Validate(instance);
    public Task<Result> ValidateAsync(OrderLineCommand instance, CancellationToken cancellationToken = default) => Task.FromResult(Validate(instance));
    public Task<Result> ValidateAsync(OrderLineCommand instance, string ruleSet, CancellationToken cancellationToken = default) => Task.FromResult(Validate(instance));
    public Task<Result> ValidateAsync(OrderLineCommand instance, ValidationOptions options) => Task.FromResult(Validate(instance));
}

public sealed class OrderWithDetaillessValidator : AbstractValidator<OrderCommand>
{
    public OrderWithDetaillessValidator()
    {
        RuleForEach(x => x.Items)
            .SetValidator(new DetaillessOrderLineValidator());
    }
}

public sealed class OrderWithResourceValidator : AbstractValidator<OrderCommand>
{
    public OrderWithResourceValidator()
    {
        RuleForEach(x => x.Items)
            .SetValidator(new ResourceOrderLineValidator());
    }
}

public sealed class OrderWithDetailedResourceValidator : AbstractValidator<OrderCommand>
{
    public OrderWithDetailedResourceValidator()
    {
        RuleForEach(x => x.Items)
            .SetValidator(new DetailedResourceOrderLineValidator());
    }
}

public class CollectionRuleTests
{
    [Test]
    public async Task Validate_CollectionValidator_UsesIndexedPaths()
    {
        Result result = new OrderValidator().Validate(new OrderCommand([new OrderLineCommand(""), new OrderLineCommand("ABC")]));

        await Assert.That(result.TryGetError(out var error)).IsTrue();
        await Assert.That(error.Details).Contains(new ErrorDetail("Items[0].Sku", "Sku must not be empty"));
    }

    [Test]
    public async Task Validate_CollectionValidator_SkipsNullItems()
    {
        Result result = new OrderValidator().Validate(new OrderCommand([null!, new OrderLineCommand("ABC") ]));

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Validate_CollectionValidator_PreservesChildErrorsWithoutDetails()
    {
        Result result = new OrderWithDetaillessValidator().Validate(new OrderCommand([new OrderLineCommand("ABC")]));

        await Assert.That(result.TryGetError(out var error)).IsTrue();
        await Assert.That(error).IsTypeOf<Error.Application.InvalidOperationError>();
        await Assert.That(error.Code).IsEqualTo(Error.Application.InvalidOperation().Code);
        await Assert.That(error.Message).IsEqualTo("Collection child validator failed without details");
    }

    [Test]
    public async Task Validate_CollectionValidator_PreservesNonValidationChildErrorFamilies()
    {
        Result result = new OrderWithResourceValidator().Validate(new OrderCommand([new OrderLineCommand("ABC")]));

        await Assert.That(result.TryGetError(out var error)).IsTrue();
        await Assert.That(error).IsTypeOf<Error.Resource.NotFoundError>();
        await Assert.That(error.Code).IsEqualTo(Error.Resource.NotFound().Code);
        await Assert.That(error.Message).IsEqualTo("Order line resource was not found");
    }

    [Test]
    public async Task Validate_CollectionValidator_PrefixesDetailedTypedChildErrors()
    {
        Result result = new OrderWithDetailedResourceValidator().Validate(new OrderCommand([new OrderLineCommand("ABC")]));

        await Assert.That(result.TryGetError(out var error)).IsTrue();
        await Assert.That(error).IsTypeOf<Error.Resource.NotFoundError>();
        await Assert.That(error.Details).Contains(new ErrorDetail("Items[0].Sku", "Sku could not be resolved"));
    }
}
