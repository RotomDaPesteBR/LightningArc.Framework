using LightningArc.Results;
using LightningArc.Validations;

namespace LightningArc.Validations.Tests.Rules;

public sealed record AddressCommand(string ZipCode, string Street);
public sealed record UserCommand(string Name, AddressCommand Address);

public sealed class AddressValidator : AbstractValidator<AddressCommand>
{
    public AddressValidator()
    {
        RuleFor(x => x.ZipCode)
            .NotEmpty();
    }
}

public sealed class FailFastAddressValidator : AbstractValidator<AddressCommand>
{
    public FailFastAddressValidator()
    {
        UseFailFast();

        RuleFor(x => x.ZipCode)
            .NotEmpty();

        RuleFor(x => x.Street)
            .NotEmpty();
    }
}

public sealed class UserValidator : AbstractValidator<UserCommand>
{
    public UserValidator()
    {
        RuleFor(x => x.Address)
            .SetValidator(new AddressValidator());
    }
}

public sealed class UserWithFailFastAddressValidator : AbstractValidator<UserCommand>
{
    public UserWithFailFastAddressValidator()
    {
        RuleFor(x => x.Address)
            .SetValidator(new FailFastAddressValidator());
    }
}

public sealed class DetaillessAddressValidator : IValidator<AddressCommand>
{
    public Result Validate(AddressCommand instance) => Error.Application.InvalidOperation("Child validator failed without details");
}

public sealed class ResourceAddressValidator : IValidator<AddressCommand>
{
    public Result Validate(AddressCommand instance) => Error.Resource.NotFound("Address resource was not found");
}

public sealed class DetailedResourceAddressValidator : IValidator<AddressCommand>
{
    public Result Validate(AddressCommand instance) => Error.Resource.NotFound(
        "Address resource was not found",
        [new ErrorDetail(nameof(AddressCommand.ZipCode), "Zip code could not be resolved")]);
}

public sealed class UserWithDetaillessAddressValidator : AbstractValidator<UserCommand>
{
    public UserWithDetaillessAddressValidator()
    {
        RuleFor(x => x.Address)
            .SetValidator(new DetaillessAddressValidator());
    }
}

public sealed class UserWithResourceAddressValidator : AbstractValidator<UserCommand>
{
    public UserWithResourceAddressValidator()
    {
        RuleFor(x => x.Address)
            .SetValidator(new ResourceAddressValidator());
    }
}

public sealed class UserWithDetailedResourceAddressValidator : AbstractValidator<UserCommand>
{
    public UserWithDetailedResourceAddressValidator()
    {
        RuleFor(x => x.Address)
            .SetValidator(new DetailedResourceAddressValidator());
    }
}

public class NestedValidatorTests
{
    [Test]
    public async Task Validate_NestedValidator_PrefixesChildPaths()
    {
        Result result = new UserValidator().Validate(new UserCommand("Alice", new AddressCommand(string.Empty, "Main St")));

        await Assert.That(result.TryGetError(out var error)).IsTrue();
        await Assert.That(error.Details).Contains(new ErrorDetail("Address.ZipCode", "ZipCode must not be empty"));
    }

    [Test]
    public async Task Validate_NestedValidator_PreservesChildFailFastBehavior()
    {
        Result result = new UserWithFailFastAddressValidator().Validate(
            new UserCommand("Alice", new AddressCommand(string.Empty, string.Empty)));

        await Assert.That(result.TryGetError(out var error)).IsTrue();
        await Assert.That(error).IsNotTypeOf<AggregateError>();
        await Assert.That(error.Details).Contains(new ErrorDetail("Address.ZipCode", "ZipCode must not be empty"));
        await Assert.That(error.Details.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Validate_NestedValidator_PreservesChildErrorsWithoutDetails()
    {
        Result result = new UserWithDetaillessAddressValidator().Validate(
            new UserCommand("Alice", new AddressCommand("12345", "Main St")));

        await Assert.That(result.TryGetError(out var error)).IsTrue();
        await Assert.That(error).IsTypeOf<Error.Application.InvalidOperationError>();
        await Assert.That(error.Code).IsEqualTo(Error.Application.InvalidOperation().Code);
        await Assert.That(error.Message).IsEqualTo("Child validator failed without details");
    }

    [Test]
    public async Task Validate_NestedValidator_PreservesNonValidationChildErrorFamilies()
    {
        Result result = new UserWithResourceAddressValidator().Validate(
            new UserCommand("Alice", new AddressCommand("12345", "Main St")));

        await Assert.That(result.TryGetError(out var error)).IsTrue();
        await Assert.That(error).IsTypeOf<Error.Resource.NotFoundError>();
        await Assert.That(error.Code).IsEqualTo(Error.Resource.NotFound().Code);
        await Assert.That(error.Message).IsEqualTo("Address resource was not found");
    }

    [Test]
    public async Task Validate_NestedValidator_PrefixesDetailedTypedChildErrors()
    {
        Result result = new UserWithDetailedResourceAddressValidator().Validate(
            new UserCommand("Alice", new AddressCommand("12345", "Main St")));

        await Assert.That(result.TryGetError(out var error)).IsTrue();
        await Assert.That(error).IsTypeOf<Error.Resource.NotFoundError>();
        await Assert.That(error.Details).Contains(new ErrorDetail("Address.ZipCode", "Zip code could not be resolved"));
    }
}
