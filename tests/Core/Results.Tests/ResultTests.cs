using LightningArc.Results;
using LightningArc.Results.Successes;

namespace LightningArc.Results.Tests
{
    public class ResultTests
    {
        private static readonly Error TestError = Error.Application.Internal("Test Error");

        [Test]
        public async Task IsSuccess_ShouldBeTrue_ForSuccessResult()
        {
            // Arrange
            Result result = Result.Success();

            // Assert
            await Assert.That(result.IsSuccess).IsTrue();
            await Assert.That(result.IsFailure).IsFalse();
        }

        [Test]
        public async Task IsFailure_ShouldBeTrue_ForFailureResult()
        {
            // Arrange
            Result result = Result.Failure(TestError);

            // Assert
            await Assert.That(result.IsSuccess).IsFalse();
            await Assert.That(result.IsFailure).IsTrue();
        }

        [Test]
        public async Task Error_ShouldThrowException_WhenResultIsSuccess()
        {
            // Arrange
            Result result = Result.Success();

            // Act & Assert
            await Assert.That(() => result.Error).Throws<ResultAccessFailedException>();
        }

        [Test]
        public async Task SuccessDetails_ShouldThrowException_WhenResultIsFailure()
        {
            // Arrange
            Result result = Result.Failure(TestError);

            // Act & Assert
            await Assert.That(() => result.SuccessState).Throws<ResultAccessFailedException>();
        }

        [Test]
        public async Task Value_ShouldThrowException_WhenResultIsFailure()
        {
            // Arrange
            var result = Result<int>.Failure(TestError);

            // Act & Assert
            await Assert.That(() => result.Value).Throws<ResultAccessFailedException>();
        }

        [Test]
        public async Task ImplicitConversion_FromError_ShouldCreateFailureResult()
        {
            // Arrange
            Result result = TestError;

            // Assert
            await Assert.That(result.IsFailure).IsTrue();
            await Assert.That(result.Error).IsEqualTo(TestError);
        }

        [Test]
        public async Task ImplicitConversion_FromValue_ShouldCreateSuccessResult()
        {
            // Arrange
            const int value = 42;
            Result<int> result = value;

            // Assert
            await Assert.That(result.IsSuccess).IsTrue();
            await Assert.That(result.Value).IsEqualTo(value);
        }

        [Test]
        public async Task Success_FactoryMethods_ShouldCreateSuccessResults()
        {
            // Assert
            await Assert.That(Result.Success().SuccessState).IsTypeOf<OkSuccess>();
            await Assert.That(Result.Created().SuccessState).IsTypeOf<CreatedSuccess>();
            await Assert.That(Result.Accepted().SuccessState).IsTypeOf<AcceptedSuccess>();
            await Assert.That(Result.NoContent().SuccessState).IsTypeOf<NoContentSuccess>();
        }

        [Test]
        public async Task SuccessOfT_FactoryMethods_ShouldCreateSuccessResults()
        {
            // Arrange
            const string value = "test";

            // Assert
            await Assert.That(Result.Success(value).SuccessState).IsTypeOf<OkSuccess<string>>();
            await Assert
                .That(Result.Created(value).SuccessState)
                .IsTypeOf<CreatedSuccess<string>>();
            await Assert
                .That(Result.Accepted(value).SuccessState)
                .IsTypeOf<AcceptedSuccess<string>>();
        }

        [Test]
        public async Task Code_ShouldReturnCorrectCode_ForSuccess()
        {
            // Arrange
            Result okResult = Result.Success();
            Result createdResult = Result.Created();

            // Assert
            await Assert.That(okResult.Code).IsEqualTo(Success.Ok().Code);
            await Assert.That(createdResult.Code).IsEqualTo(Success.Created().Code);
        }

        [Test]
        public async Task Code_ShouldReturnCorrectCode_ForFailure()
        {
            // Arrange
            Result result = Result.Failure(TestError);

            // Assert
            await Assert.That(result.Code).IsEqualTo(TestError.Code);
        }

        [Test]
        public async Task Value_ShouldReturnCorrectValue_ForSuccessResultOfT()
        {
            // Arrange
            const double value = 3.14;
            var result = Result.Success(value);

            // Assert
            await Assert.That(result.Value).IsEqualTo(value);
        }
    }
}
