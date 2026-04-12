using System.Security.Authentication;
using LightningArc.Results.Exceptions;

namespace LightningArc.Results.Tests.Exceptions;

public class ExceptionMapperTests
{
    [Test]
    public async Task Map_ArgumentNullException_WithParam_ShouldMapToSpecificMessage()
    {
        // Arrange
        ArgumentNullException ex = new("testParam");

        // Act
        Error error = ExceptionMapper.Map(ex);

        // Assert
        await Assert.That(error.Code).IsEqualTo(Error.Validation.CodePrefix * 1000 + (int)Error.Validation.Codes.MissingField);
        await Assert.That(error.Message).Contains("testParam");
    }

    [Test]
    public async Task Map_AggregateException_ShouldReturnAggregateError()
    {
        // Arrange
        AggregateException ex = new(
            new ArgumentNullException("param1"),
            new UnauthorizedAccessException()
        );

        // Act
        Error error = ExceptionMapper.Map(ex);

        // Assert
        await Assert.That(error is AggregateError).IsTrue();
        AggregateError aggregate = (AggregateError)error;
        await Assert.That(aggregate.Errors).Count().IsEqualTo(2);
    }

    [Test]
    public async Task Map_FormatException_ShouldMapToValidationInvalidFormat()
    {
        // Arrange
        FormatException ex = new();

        // Act
        Error error = ExceptionMapper.Map(ex);

        // Assert
        await Assert.That(error.Code).IsEqualTo(Error.Validation.CodePrefix * 1000 + (int)Error.Validation.Codes.InvalidFormat);
    }

    [Test]
    public async Task Map_DivideByZeroException_ShouldMapToApplicationInvalidOperation()
    {
        // Arrange
        DivideByZeroException ex = new();

        // Act
        Error error = ExceptionMapper.Map(ex);

        // Assert
        await Assert.That(error.Code).IsEqualTo(Error.Application.CodePrefix * 1000 + (int)Error.Application.Codes.InvalidOperation);
    }

    [Test]
    public async Task Map_KeyNotFoundException_ShouldMapToResourceNotFound()
    {
        // Arrange
        KeyNotFoundException ex = new();

        // Act
        Error error = ExceptionMapper.Map(ex);

        // Assert
        await Assert.That(error.Code).IsEqualTo(Error.Resource.CodePrefix * 1000 + (int)Error.Resource.Codes.NotFound);
    }

    [Test]
    public async Task Register_CustomMapper_ShouldOverrideDefault()
    {
        // Arrange
        InvalidOperationException ex = new("Custom message");
        ExceptionMapper.Register<InvalidOperationException>(_ => Error.Application.Internal("Custom Error"));

        // Act
        Error error = ExceptionMapper.Map(ex);

        // Assert
        await Assert.That(error.Message).IsEqualTo("Custom Error");
    }
}
