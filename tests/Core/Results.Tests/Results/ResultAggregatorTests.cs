using LightningArc.Primitives.ValueObjects;

namespace LightningArc.Results.Tests.Results;

public class ResultAggregatorTests
{
    [Test]
    public async Task Build_NoChecks_ShouldReturnSuccess()
    {
        // Act
        Result result = Result.Aggregate().Build();

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Build_AllSuccessfulChecks_ShouldReturnSuccess()
    {
        // Act
        Result result = Result.Aggregate()
            .Check(() => Result.Success())
            .Check(() => Result.Success())
            .Build();

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Build_SingleFailure_ShouldReturnFailure()
    {
        // Act
        Result result = Result.Aggregate()
            .Check(() => Result.Failure(Error.Validation.MissingField("field1")))
            .Build();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error.Code).IsEqualTo(Error.Validation.CodePrefix * 1000 + (int)Error.Validation.Codes.MissingField);
    }

    [Test]
    public async Task Build_MultipleFailures_ShouldReturnAggregateError()
    {
        // Act
        Result result = Result.Aggregate()
            .Check(() => Result.Failure(Error.Validation.MissingField("field1")))
            .Check(() => Result.Failure(Error.Validation.InvalidParameter("field2")))
            .Build();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        // The error system combines errors using AggregateError
        AggregateError aggregateError = (AggregateError)result.Error;
        await Assert.That(aggregateError.Errors).Count().IsEqualTo(2);
    }

    [Test]
    public async Task Check_WithException_ShouldMapToError()
    {
        // Act
        Result result = Result.Aggregate()
            .Check(() => throw new ArgumentNullException("testParam"))
            .Build();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Message).Contains("testParam");
    }

    [Test]
    public async Task CheckWithValue_ShouldCaptureValueOnSuccess()
    {
        // Act
        ResultAggregator aggregator = Result.Aggregate()
            .Check(() => Email.Create("test@example.com"), out Email? email);
        
        Result result = aggregator.Build();

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(email).IsNotNull();
        await Assert.That(email!.Value).IsEqualTo("test@example.com");
    }

    [Test]
    public async Task CheckWithValue_ShouldHandleExceptionAndSetNull()
    {
        // Act
        ResultAggregator aggregator = Result.Aggregate()
            .Check(() => Email.Create("invalid"), out Email? email);
        
        Result result = aggregator.Build();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(email is null).IsTrue();
    }
}
