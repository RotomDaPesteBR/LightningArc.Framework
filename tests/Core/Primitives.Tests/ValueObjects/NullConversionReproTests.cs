using LightningArc.Primitives.ValueObjects;

namespace LightningArc.Primitives.Tests.ValueObjects
{
    public class NullConversionReproTests
    {
        [Test]
        public async Task Url_ImplicitConversion_WhenNull_ShouldThrowInvalidOperationException()
        {
            // Arrange
            Url? url = null;

            // Act & Assert
            await Assert.That(() => { string result = url!; }).Throws<InvalidOperationException>()
                .WithMessage("Cannot convert a null ValueObject to string.");
        }

        [Test]
        public async Task Cpf_ImplicitConversion_WhenNull_ShouldThrowInvalidOperationException()
        {
            // Arrange
            Cpf? cpf = null;

            // Act & Assert
            await Assert.That(() => { string result = cpf!; }).Throws<InvalidOperationException>()
                .WithMessage("Cannot convert a null ValueObject to string.");
        }

        [Test]
        public async Task Cnpj_ImplicitConversion_WhenNull_ShouldThrowInvalidOperationException()
        {
            // Arrange
            Cnpj? cnpj = null;

            // Act & Assert
            await Assert.That(() => { string result = cnpj!; }).Throws<InvalidOperationException>()
                .WithMessage("Cannot convert a null ValueObject to string.");
        }

        [Test]
        public async Task PhoneNumber_ImplicitConversion_WhenNull_ShouldThrowInvalidOperationException()
        {
            // Arrange
            PhoneNumber? phone = null;

            // Act & Assert
            await Assert.That(() => { string result = phone!; }).Throws<InvalidOperationException>()
                .WithMessage("Cannot convert a null ValueObject to string.");
        }

        [Test]
        public async Task Email_ImplicitConversion_WhenNull_ShouldThrowInvalidOperationException()
        {
            // Arrange
            Email? email = null;

            // Act & Assert
            await Assert.That(() => { string result = email!; }).Throws<InvalidOperationException>()
                .WithMessage("Cannot convert a null ValueObject to string.");
        }
    }
}
