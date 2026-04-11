using LightningArc.Results;

namespace LightningArc.Results.Tests
{
    public class EqualityTests
    {
        [Test]
        public async Task Error_SameCodeAndDetails_ShouldBeEqual()
        {
            // Arrange
            Error error1 = Error.Validation.InvalidParameter("Same message", new ErrorDetail("Field", "Error"));
            Error error2 = Error.Validation.InvalidParameter("Different message", new ErrorDetail("Field", "Error"));

            // Assert
            await Assert.That(error1).IsEqualTo(error2);
            await Assert.That(error1 == error2).IsTrue();
        }

        [Test]
        public async Task Error_DifferentDetails_ShouldNotBeEqual()
        {
            // Arrange
            Error error1 = Error.Validation.InvalidParameter("Msg", new ErrorDetail("Field1", "Error"));
            Error error2 = Error.Validation.InvalidParameter("Msg", new ErrorDetail("Field2", "Error"));

            // Assert
            await Assert.That(error1).IsNotEqualTo(error2);
            await Assert.That(error1 != error2).IsTrue();
        }

        [Test]
        public async Task Success_SameCode_ShouldBeEqual()
        {
            // Arrange
            Success success1 = Success.Ok("Msg 1");
            Success success2 = Success.Ok("Msg 2");

            // Assert
            await Assert.That(success1).IsEqualTo(success2);
        }

        [Test]
        public async Task SuccessWithValue_SameValue_ShouldBeEqual()
        {
            // Arrange
            var success1 = Success.Ok(10);
            var success2 = Success.Ok(10);

            // Assert
            await Assert.That(success1).IsEqualTo(success2);
        }

        [Test]
        public async Task Result_SameError_ShouldBeEqual()
        {
            // Arrange
            Error error = Error.Application.Internal("Error");
            Result result1 = error;
            Result result2 = error;

            // Assert
            await Assert.That(result1).IsEqualTo(result2);
        }

        [Test]
        public async Task ResultGeneric_SameValue_ShouldBeEqual()
        {
            // Arrange
            Result<int> result1 = 100;
            Result<int> result2 = 100;

            // Assert
            await Assert.That(result1).IsEqualTo(result2);
        }

        [Test]
        public async Task ResultGeneric_DifferentValue_ShouldNotBeEqual()
        {
            // Arrange
            Result<int> result1 = 100;
            Result<int> result2 = 200;

            // Assert
            await Assert.That(result1).IsNotEqualTo(result2);
        }
    }
}

