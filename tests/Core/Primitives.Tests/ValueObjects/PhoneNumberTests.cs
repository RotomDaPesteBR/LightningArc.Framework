using LightningArc.Primitives.ValueObjects;

namespace LightningArc.Primitives.Tests.ValueObjects
{
    public class PhoneNumberTests
    {
        [Test]
        [Arguments("+5511999999999")]
        [Arguments("11999999999")]
        [Arguments("(11) 99999-9999")]
        public async Task Create_ValidPhoneNumber_ShouldCreateInstance(string phoneValue)
        {
            // Act
            var phone = PhoneNumber.Create(phoneValue);

            // Assert
            var expected = new string(phoneValue.Where(char.IsDigit).ToArray());
            await Assert.That(phone.Value).IsEqualTo(expected);
        }

        [Test]
        [Arguments("123")] // Too short
        [Arguments("1234567890123456")] // Too long (max 15 digits according to E.164)
        [Arguments("abc")] // Non-numeric
        [Arguments("")]
        [Arguments(null)]
        public async Task Create_InvalidPhoneNumber_ShouldThrowArgumentException(string? invalidPhone)
        {
            // Act & Assert
            await Assert.That(() => PhoneNumber.Create(invalidPhone!)).Throws<ArgumentException>();
        }

        [Test]
        public async Task TryCreate_ValidPhoneNumber_ShouldReturnTrueAndResult()
        {
            // Arrange
            const string phoneValue = "11999999999";

            // Act
            var success = PhoneNumber.TryCreate(phoneValue, out var result);

            // Assert
            await Assert.That(success).IsTrue();
            await Assert.That(result).IsNotNull();
            await Assert.That(result!.Value).IsEqualTo(phoneValue);
        }

        [Test]
        public async Task TryCreate_InvalidPhoneNumber_ShouldReturnFalseAndNull()
        {
            // Arrange
            const string phoneValue = "123";

            // Act
            var success = PhoneNumber.TryCreate(phoneValue, out var result);

            // Assert
            await Assert.That(success).IsFalse();
            await Assert.That(result is null).IsTrue();
        }

        [Test]
        public async Task ImplicitConversion_ToString_ShouldReturnStringValue()
        {
            // Arrange
            const string phoneValue = "11999999999";
            var phone = PhoneNumber.Create(phoneValue);

            // Act
            string result = phone;

            // Assert
            await Assert.That(result).IsEqualTo(phoneValue);
        }

        [Test]
        public async Task ImplicitConversion_FromString_ShouldCreatePhoneNumber()
        {
            // Arrange
            const string phoneValue = "11999999999";

            // Act
            PhoneNumber phone = phoneValue;

            // Assert
            await Assert.That(phone.Value).IsEqualTo(phoneValue);
        }

        [Test]
        public async Task ImplicitConversion_NullToString_ShouldThrowInvalidOperationException()
        {
            // Arrange
            PhoneNumber? phone = null;

            // Act & Assert
            await Assert.That(() => (string)phone!).Throws<InvalidOperationException>();
        }
    }
}
