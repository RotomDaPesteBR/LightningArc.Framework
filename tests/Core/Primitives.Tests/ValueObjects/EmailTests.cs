using LightningArc.Primitives.ValueObjects;

namespace LightningArc.Primitives.Tests.ValueObjects
{
    public class EmailTests
    {
        [Test]
        public async Task Create_ValidEmail_ShouldCreateInstance()
        {
            // Arrange
            const string emailValue = "test@example.com";

            // Act
            Email email = Email.Create(emailValue);

            // Assert
            await Assert.That(email.Value).IsEqualTo(emailValue);
        }

        [Test]
        [Arguments("invalid-email")]
        [Arguments("test@")]
        [Arguments("@example.com")]
        [Arguments("test@example")]
        public async Task Create_InvalidEmail_ShouldThrowArgumentException(string invalidEmail)
        {
            // Act & Assert
            await Assert.That(() => Email.Create(invalidEmail)).Throws<ArgumentException>();
        }

        [Test]
        public async Task ImplicitConversion_ToString_ShouldReturnStringValue()
        {
            // Arrange
            const string emailValue = "test@example.com";
            Email email = Email.Create(emailValue);

            // Act
            string result = email;

            // Assert
            await Assert.That(result).IsEqualTo(emailValue);
        }

        [Test]
        public async Task ImplicitConversion_FromString_ShouldCreateEmail()
        {
            // Arrange
            const string emailValue = "test@example.com";

            // Act
            Email email = emailValue;

            // Assert
            await Assert.That(email.Value).IsEqualTo(emailValue);
        }

        [Test]
        public async Task Equality_SameValue_ShouldBeEqual()
        {
            // Arrange
            Email email1 = Email.Create("test@example.com");
            Email email2 = Email.Create("test@example.com");

            // Assert
            await Assert.That(email1).IsEqualTo(email2);
            await Assert.That(email1 == email2).IsTrue();
        }
    }
}

