using LightningArc.Primitives.ValueObjects;

namespace LightningArc.Primitives.Tests.ValueObjects
{
    public class CnpjTests
    {
        [Test]
        [Arguments("00000000000191")]
        [Arguments("00.000.000/0001-91")]
        public async Task Create_ValidCnpj_ShouldCreateInstance(string cnpjValue)
        {
            // Act
            Cnpj cnpj = Cnpj.Create(cnpjValue);

            // Assert
            string expected = cnpjValue.Replace(".", "").Replace("-", "").Replace("/", "");
            await Assert.That(cnpj.Value).IsEqualTo(expected);
        }

        [Test]
        [Arguments("00000000000100")] // Invalid check digits
        [Arguments("11111111111111")] // All equal digits
        [Arguments("123")]             // Too short
        [Arguments("123456789012345")] // Too long
        [Arguments("abc12345678901")]  // Non-numeric
        public async Task Create_InvalidCnpj_ShouldThrowArgumentException(string invalidCnpj)
        {
            // Act & Assert
            await Assert.That(() => Cnpj.Create(invalidCnpj)).Throws<ArgumentException>();
        }

        [Test]
        public async Task TryCreate_ValidCnpj_ShouldReturnTrueAndResult()
        {
            // Arrange
            const string cnpjValue = "00000000000191";

            // Act
            bool success = Cnpj.TryCreate(cnpjValue, out Cnpj? result);

            // Assert
            await Assert.That(success).IsTrue();
            await Assert.That(result).IsNotNull();
            await Assert.That(result!.Value).IsEqualTo(cnpjValue);
        }

        [Test]
        public async Task TryCreate_InvalidCnpj_ShouldReturnFalseAndNull()
        {
            // Arrange
            const string cnpjValue = "00000000000100";

            // Act
            bool success = Cnpj.TryCreate(cnpjValue, out Cnpj? result);

            // Assert
            await Assert.That(success).IsFalse();
            await Assert.That(result is null).IsTrue();
        }

        [Test]
        public async Task ImplicitConversion_ToString_ShouldReturnStringValue()
        {
            // Arrange
            const string cnpjValue = "00000000000191";
            Cnpj cnpj = Cnpj.Create(cnpjValue);

            // Act
            string result = cnpj;

            // Assert
            await Assert.That(result).IsEqualTo(cnpjValue);
        }

        [Test]
        public async Task ImplicitConversion_FromString_ShouldCreateCnpj()
        {
            // Arrange
            const string cnpjValue = "00000000000191";

            // Act
            Cnpj cnpj = cnpjValue;

            // Assert
            await Assert.That(cnpj.Value).IsEqualTo(cnpjValue);
        }

        [Test]
        public async Task ImplicitConversion_NullToString_ShouldThrowInvalidOperationException()
        {
            // Arrange
            Cnpj? cnpj = null;

            // Act & Assert
            await Assert.That(() => (string)cnpj!).Throws<InvalidOperationException>();
        }
    }
}
