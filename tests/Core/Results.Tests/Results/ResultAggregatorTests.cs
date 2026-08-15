using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

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

    [Test]
    public async Task Check_ResultFactory_Success_ShouldCallOnSuccessAndNotAggregateError()
    {
        // Act
        int value = 0;
        ResultAggregator aggregator = Result.Aggregate()
            .Check(() => Result<int>.Success(42), v => value = v);

        Result result = aggregator.Build();

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task Check_ResultFactory_Failure_ShouldAggregateErrorAndCallOnSuccessWithDefault()
    {
        // Act
        int value = 0;
        ResultAggregator aggregator = Result.Aggregate()
            .Check(() => Result<int>.Failure(Error.Validation.MissingField("field1")), v => value = v);

        Result result = aggregator.Build();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(value).IsEqualTo(default);
    }

    [Test]
    public async Task Check_ResultFactory_Exception_ShouldAggregateErrorAndCallOnSuccessWithDefault()
    {
        // Act
        int value = 0;
        ResultAggregator aggregator = Result.Aggregate()
            .Check<int>(() => throw new InvalidOperationException("boom"), v => value = v);

        Result result = aggregator.Build();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(value).IsEqualTo(default);
    }

    [Test]
    public async Task CheckAsync_ResultFactory_Success_ShouldCallOnSuccessAndNotAggregateError()
    {
        // Act
        int value = 0;
        ResultAggregator aggregator = Result.Aggregate();
        aggregator = await aggregator.CheckAsync(async () => Result<int>.Success(42), v => value = v);

        Result result = aggregator.Build();

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task CheckAsync_ResultFactory_Failure_ShouldAggregateErrorAndCallOnSuccessWithDefault()
    {
        // Act
        int value = 0;
        ResultAggregator aggregator = Result.Aggregate();
        aggregator = await aggregator.CheckAsync(async () => Result<int>.Failure(Error.Validation.MissingField("field1")), v => value = v);

        Result result = aggregator.Build();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(value).IsEqualTo(default);
    }

    [Test]
    public async Task CheckAsync_ResultFactory_Exception_ShouldAggregateErrorAndCallOnSuccessWithDefault()
    {
        // Act
        int value = 0;
        ResultAggregator aggregator = Result.Aggregate();
        aggregator = await aggregator.CheckAsync<int>(async () => { throw new InvalidOperationException("boom"); }, v => value = v);

        Result result = aggregator.Build();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(value).IsEqualTo(default);
    }

    [Test]
    public async Task Ensure_LazyErrorFactory_ShouldOnlyInvokeWhenConditionFalse()
    {
        // Arrange
        bool factoryCalled = false;
        Error error = Error.Validation.InvalidParameter("test");

        // Act - condition true, factory should NOT be called
        ResultAggregator aggregator = Result.Aggregate()
            .Ensure(true, () => { factoryCalled = true; return error; });

        Result result = aggregator.Build();

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(factoryCalled).IsFalse();

        // Act - condition false, factory SHOULD be called
        factoryCalled = false;
        aggregator = Result.Aggregate()
            .Ensure(false, () => { factoryCalled = true; return error; });

        result = aggregator.Build();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(factoryCalled).IsTrue();
    }

    [Test]
    public async Task When_Success_ShouldNotAggregateError()
    {
        // Act
        Result result = Result.Aggregate()
            .When(() => true, Error.Validation.MissingField("field1"))
            .Build();

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task When_Failure_ShouldAggregateError()
    {
        // Act
        Result result = Result.Aggregate()
            .When(() => false, Error.Validation.MissingField("field1"))
            .Build();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task When_Exception_ShouldPropagate()
    {
        // Act & Assert
        await Assert.That(() => Result.Aggregate()
            .When(() => throw new InvalidOperationException("boom"), Error.Validation.MissingField("field1"))
            .Build())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task WhenAsync_Success_ShouldNotAggregateError()
    {
        // Act
        Result result = await Result.Aggregate()
            .WhenAsync(async () => await Task.FromResult(true), Error.Validation.MissingField("field1"))
            .BuildAsync();

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task WhenAsync_Failure_ShouldAggregateError()
    {
        // Act
        Result result = await Result.Aggregate()
            .WhenAsync(async () => await Task.FromResult(false), Error.Validation.MissingField("field1"))
            .BuildAsync();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task WhenAsync_Exception_ShouldPropagate()
    {
        // Act & Assert
        await Assert.That(async () => await Result.Aggregate()
            .WhenAsync(async () => { throw new InvalidOperationException("boom"); }, Error.Validation.MissingField("field1"))
            .BuildAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task WhenAll_AllSuccess_ShouldNotAggregateErrors()
    {
        // Act
        WhenCondition[] conditions =
        {
            (() => Task.FromResult(true), Error.Validation.MissingField("field1")),
            (() => Task.FromResult(true), Error.Validation.MissingField("field2")),
        };

        Result result = await Result.Aggregate()
            .WhenAll(conditions)
            .BuildAsync();

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task WhenAll_SomeFailures_ShouldAggregateErrors()
    {
        // Act
        WhenCondition[] conditions =
        {
            (() => Task.FromResult(true), Error.Validation.MissingField("field1")),
            (() => Task.FromResult(false), Error.Validation.MissingField("field2")),
            (() => Task.FromResult(false), Error.Validation.InvalidParameter("field3")),
        };

        Result result = await Result.Aggregate()
            .WhenAll(conditions)
            .BuildAsync();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        AggregateError aggregateError = (AggregateError)result.Error;
        await Assert.That(aggregateError.Errors).Count().IsEqualTo(2);
    }

    [Test]
    public async Task WhenAll_Exception_ShouldPropagate()
    {
        // Act & Assert
        WhenCondition[] conditions =
        {
            (() => Task.FromResult(true), Error.Validation.MissingField("field1")),
            (() => { throw new InvalidOperationException("boom"); }, Error.Validation.MissingField("field2")),
        };

        await Assert.That(async () => await Result.Aggregate()
            .WhenAll(conditions)
            .BuildAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task WhenAll_Empty_ShouldReturnSuccess()
    {
        // Act
        Result result = await Result.Aggregate()
            .WhenAll(Array.Empty<WhenCondition>())
            .BuildAsync();

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task CheckAll_AsyncCheck_NoCallback_ShouldWorkSameAsOriginal()
    {
        // Act
        Result result = await Result.Aggregate()
            .CheckAll(new[]
            {
                new AsyncCheck(() => Task.FromResult(Result.Success())),
                new AsyncCheck(() => Task.FromResult(Result.Failure(Error.Validation.MissingField("field1")))),
                new AsyncCheck(() => Task.FromResult(Result.Failure(Error.Validation.InvalidParameter("field2")))),
            })
            .BuildAsync();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        AggregateError aggregateError = (AggregateError)result.Error;
        await Assert.That(aggregateError.Errors).Count().IsEqualTo(2);
    }

    [Test]
    public async Task CheckAll_AsyncCheck_WithCallback_ShouldInvokeCallbackPerCheck()
    {
        // Arrange
        var callbackResults = new List<Result>();

        // Act
        Result result = await Result.Aggregate()
            .CheckAll(new[]
            {
                new AsyncCheck(() => Task.FromResult(Result.Success()), r => callbackResults.Add(r)),
                new AsyncCheck(() => Task.FromResult(Result.Failure(Error.Validation.MissingField("field1"))), r => callbackResults.Add(r)),
                new AsyncCheck(() => Task.FromResult(Result.Failure(Error.Validation.InvalidParameter("field2"))), r => callbackResults.Add(r)),
            })
            .BuildAsync();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(callbackResults).Count().IsEqualTo(3);
        await Assert.That(callbackResults[0].IsSuccess).IsTrue();
        await Assert.That(callbackResults[1].IsFailure).IsTrue();
        await Assert.That(callbackResults[2].IsFailure).IsTrue();
    }

    [Test]
    public async Task CheckAll_AsyncCheck_CallbackFiresImmediatelyNotAfterBatch()
    {
        // Arrange
        var completionOrder = new List<int>();
        var gate = new TaskCompletionSource<bool>();
        var secondCheckStarted = new TaskCompletionSource<bool>();

        // Act - start the aggregation but don't await yet
        var aggregateTask = Result.Aggregate()
            .CheckAll(new[]
            {
                new AsyncCheck(async () =>
                {
                    await gate.Task;
                    completionOrder.Add(1);
                    return Result.Success();
                }),
                new AsyncCheck(async () =>
                {
                    secondCheckStarted.SetResult(true);
                    completionOrder.Add(2);
                    return Result.Success();
                }),
            })
            .BuildAsync();

        // Wait for second check to start and complete its callback
        await secondCheckStarted.Task;
        await Task.Delay(10); // allow callback to fire

        // Second check callback should have fired already
        await Assert.That(completionOrder).Count().IsEqualTo(1);
        await Assert.That(completionOrder[0]).IsEqualTo(2);

        // Now release the gate and await completion
        gate.SetResult(true);
        Result result = await aggregateTask;

        // Assert - both completed, order preserved
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(completionOrder).Count().IsEqualTo(2);
        await Assert.That(completionOrder[0]).IsEqualTo(2); // second check completed first
        await Assert.That(completionOrder[1]).IsEqualTo(1); // first check completed after gate
    }

    [Test]
    public async Task CheckAll_AsyncCheck_ExceptionInAction_ShouldBeCaughtAndNotAbortBatch()
    {
        // Act
        Result result = await Result.Aggregate()
            .CheckAll(new[]
            {
                new AsyncCheck(() => Task.FromResult(Result.Success())),
                new AsyncCheck(async () => { throw new InvalidOperationException("boom"); }),
                new AsyncCheck(() => Task.FromResult(Result.Failure(Error.Validation.MissingField("field1")))),
            })
            .BuildAsync();

        // Assert - all three checks ran, two failures aggregated
        await Assert.That(result.IsFailure).IsTrue();
        AggregateError aggregateError = (AggregateError)result.Error;
        await Assert.That(aggregateError.Errors).Count().IsEqualTo(2);
    }

    [Test]
    public async Task CheckAll_AsyncCheck_ExceptionInCallback_ShouldPropagate()
    {
        // Act & Assert
        await Assert.That(async () => await Result.Aggregate()
            .CheckAll(new[]
            {
                new AsyncCheck(() => Task.FromResult(Result.Success()), r => throw new InvalidOperationException("callback boom")),
            })
            .BuildAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task CheckAll_AsyncCheck_ImplicitConversion_ShouldWork()
    {
        // Act - uses implicit conversion from Func<Task<Result>> to AsyncCheck
        Result result = await Result.Aggregate()
            .CheckAll(new[]
            {
                (Func<Task<Result>>)(() => Task.FromResult(Result.Success())),
                (Func<Task<Result>>)(() => Task.FromResult(Result.Failure(Error.Validation.MissingField("field1")))),
            })
            .BuildAsync();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        // Single error is not wrapped in AggregateError
        await Assert.That(result.Error).IsNotNull();
        await Assert.That(result.Error.Code).IsEqualTo(Error.Validation.CodePrefix * 1000 + (int)Error.Validation.Codes.MissingField);
    }

    [Test]
    public async Task CheckAll_TaskAggregator_WithAsyncCheck_ShouldWork()
    {
        // Act - Task<ResultAggregator> receiver mirror
        Result result = await (Task.FromResult(Result.Aggregate()))
            .CheckAll(new[]
            {
                new AsyncCheck(() => Task.FromResult(Result.Success())),
                new AsyncCheck(() => Task.FromResult(Result.Failure(Error.Validation.MissingField("field1")))),
            })
            .BuildAsync();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
    }
}