using LightningArc.Primitives.ValueObjects;

namespace LightningArc.Primitives.Tests.ValueObjects
{
    public class CpfTests
    {
        [Test]
        public async Task Create_ValidCpf_ShouldCreateInstance()
        {
            // Arrange
            const string cpfValue = "12345678909";

            // Act
            var cpf = Cpf.Create(cpfValue);

            // Assert
            await Assert.That(cpf.Value).IsEqualTo(cpfValue);
        }

        [Test]
        [Arguments("12345678900")] // Invalid check digit
        [Arguments("123456789")] // Too short
        [Arguments("123456789012")] // Too long
        [Arguments("abcdefghijk")] // Non-numeric
        [Arguments("11111111111")] // Repeated digits
        public async Task Create_InvalidCpf_ShouldThrowArgumentException(string invalidCpf)
        {
            // Act & Assert
            await Assert.That(() => Cpf.Create(invalidCpf)).Throws<ArgumentException>();
        }

        [Test]
        public async Task TryCreate_ValidCpf_ShouldReturnTrue()
        {
            // Arrange
            const string cpfValue = "12345678909";

            // Act
            var success = Cpf.TryCreate(cpfValue, out var cpf);

            // Assert
            await Assert.That(success).IsTrue();
            await Assert.That(cpf).IsNotNull();
            await Assert.That(cpf!.Value).IsEqualTo(cpfValue);
        }

        [Test]
        public async Task TryCreate_InvalidCpf_ShouldReturnFalse()
        {
            // Arrange
            const string cpfValue = "12345678900";

            // Act
            var success = Cpf.TryCreate(cpfValue, out var cpf);

            // Assert
            await Assert.That(success).IsFalse();
            await Assert.That(cpf is null).IsTrue();
        }

        [Test]
        public async Task ImplicitConversion_ToString_ShouldReturnStringValue()
        {
            // Arrange
            const string cpfValue = "12345678909";
            var cpf = Cpf.Create(cpfValue);

            // Act
            string result = cpf;

            // Assert
            await Assert.That(result).IsEqualTo(cpfValue);
        }

        [Test]
        public async Task ImplicitConversion_FromString_ShouldCreateCpf()
        {
            // Arrange
            const string cpfValue = "12345678909";

            // Act
            Cpf cpf = cpfValue;

            // Assert
            await Assert.That(cpf.Value).IsEqualTo(cpfValue);
        }

        [Test]
        public async Task ImplicitConversion_NullToString_ShouldThrowInvalidOperationException()
        {
            // Arrange
            Cpf? cpf = null;

            // Act & Assert
            await Assert.That(() => (string)cpf!).Throws<InvalidOperationException>();
        }
    }
}
