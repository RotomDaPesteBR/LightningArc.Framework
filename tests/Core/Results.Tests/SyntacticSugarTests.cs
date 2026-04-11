using LightningArc.Results;

namespace LightningArc.Results.Tests
{
    public class SyntacticSugarTests
    {
        [Test]
        public async Task BooleanOperators_ShouldWorkCorrectly()
        {
            // Arrange
            Result success = Result.Success();
            Result failure = Error.Application.Internal();

            // Assert
            await Assert.That(success ? true : false).IsTrue();
            await Assert.That(failure ? true : false).IsFalse();
            await Assert.That(!success).IsFalse();
            await Assert.That(!failure).IsTrue();
        }

        [Test]
        public async Task Deconstruction_Result_ShouldWorkCorrectly()
        {
            // Arrange
            Error expectedError = Error.Validation.InvalidParameter("Error");
            Result result = expectedError;

            // Act
            (bool isSuccess, Error? error) = result;

            // Assert
            await Assert.That(isSuccess).IsFalse();
            await Assert.That(error).IsEqualTo(expectedError);
        }

        [Test]
        public async Task Deconstruction_ResultGeneric_ShouldWorkCorrectly()
        {
            // Arrange
            const int expectedValue = 42;
            Result<int> result = expectedValue;

            // Act
            (bool isSuccess, int value, Error? error) = result;

            // Assert
            await Assert.That(isSuccess).IsTrue();
            await Assert.That(value).IsEqualTo(expectedValue);
            await Assert.That(error).IsNull();
        }

        [Test]
        public async Task Deconstruction_Error_ShouldWorkCorrectly()
        {
            // Arrange
            Error error = Error.Validation.InvalidParameter("My Error", [new ErrorDetail("F1", "M1")]);

            // Act
            (int code, string message, var details) = error;

            // Assert
            await Assert.That(code).IsEqualTo(error.Code);
            await Assert.That(message).IsEqualTo(error.Message);
            await Assert.That(details).IsEquivalentTo(error.Details);
        }

        [Test]
        public async Task ErrorCombination_OperatorPlus_ShouldCreateAggregateError()
        {
            // Arrange
            Error error1 = Error.Validation.InvalidParameter("Error 1", [new ErrorDetail("Field1", "Required")]);
            Error error2 = Error.Application.Internal("Error 2", [new ErrorDetail("System", "Down")]);

            // Act
            Error combined = error1 + error2;

            // Assert
            await Assert.That(combined).IsTypeOf<AggregateError>();
            AggregateError aggregate = (AggregateError)combined;
            await Assert.That(aggregate.Errors.Count).IsEqualTo(2);
            await Assert.That(combined.Code).IsEqualTo(99001); // General module
            await Assert.That(combined.Details.Count).IsEqualTo(2);
        }

        [Test]
        public async Task AggregateError_Flatten_ShouldRemoveNestedAggregates()
        {
            // Arrange
            Error e1 = Error.Validation.InvalidFormat("E1");
            Error e2 = Error.Validation.InvalidFormat("E2");
            Error e3 = Error.Validation.InvalidFormat("E3");

            Error agg1 = e1 + e2; // Aggregate with [e1, e2]
            Error agg2 = agg1 + e3; // Aggregate with [agg1, e3]

            // Act
            AggregateError flattened = ((AggregateError)agg2).Flatten();

            // Assert
            await Assert.That(flattened.Errors.Count).IsEqualTo(3);
            await Assert.That(flattened.Errors).IsEquivalentTo([e1, e2, e3]);
        }

        [Test]
        public async Task ExplicitConversion_ToValue_ShouldReturnValue()
        {
            // Arrange
            Result<int> result = 42;

            // Act
            int value = (int)result;

            // Assert
            await Assert.That(value).IsEqualTo(42);
        }

        [Test]
        public async Task ExplicitConversion_ToError_ShouldReturnError()
        {
            // Arrange
            Error expectedError = Error.Application.Internal();
            Result<int> result = expectedError;

            // Act
            Error error = (Error)result;

            // Assert
            await Assert.That(error).IsEqualTo(expectedError);
        }
    }
}

