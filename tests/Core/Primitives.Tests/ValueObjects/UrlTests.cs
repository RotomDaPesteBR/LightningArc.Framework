using LightningArc.Primitives.ValueObjects;

namespace LightningArc.Primitives.Tests.ValueObjects
{
    public class UrlTests
    {
        [Test]
        [Arguments("https://google.com")]
        [Arguments("http://localhost:8080")]
        [Arguments("ftp://myserver.com")]
        public async Task Create_ValidUrl_ShouldCreateInstance(string urlValue)
        {
            // Act
            Url url = Url.Create(urlValue);

            // Assert
            await Assert.That(url.Value).IsEqualTo(urlValue);
        }

        [Test]
        [Arguments("not-a-url")]
        [Arguments("http://")]
        [Arguments("")]
        [Arguments(null)]
        public async Task Create_InvalidUrl_ShouldThrowArgumentException(string? invalidUrl)
        {
            // Act & Assert
            await Assert.That(() => Url.Create(invalidUrl!)).Throws<ArgumentException>();
        }

        [Test]
        public async Task TryCreate_ValidUrl_ShouldReturnTrueAndResult()
        {
            // Arrange
            const string urlValue = "https://lightningarc.com";

            // Act
            bool success = Url.TryCreate(urlValue, out Url? result);

            // Assert
            await Assert.That(success).IsTrue();
            await Assert.That(result).IsNotNull();
            await Assert.That(result!.Value).IsEqualTo(urlValue);
        }

        [Test]
        public async Task TryCreate_InvalidUrl_ShouldReturnFalseAndNull()
        {
            // Arrange
            const string urlValue = "invalid-url";

            // Act
            bool success = Url.TryCreate(urlValue, out Url? result);

            // Assert
            await Assert.That(success).IsFalse();
            await Assert.That(result is null).IsTrue();
        }

        [Test]
        public async Task ImplicitConversion_ToString_ShouldReturnStringValue()
        {
            // Arrange
            const string urlValue = "https://google.com";
            Url url = Url.Create(urlValue);

            // Act
            string result = url;

            // Assert
            await Assert.That(result).IsEqualTo(urlValue);
        }

        [Test]
        public async Task ImplicitConversion_FromString_ShouldCreateUrl()
        {
            // Arrange
            const string urlValue = "https://google.com";

            // Act
            Url url = urlValue;

            // Assert
            await Assert.That(url.Value).IsEqualTo(urlValue);
        }

        [Test]
        public async Task ImplicitConversion_NullToString_ShouldThrowInvalidOperationException()
        {
            // Arrange
            Url? url = null;

            // Act & Assert
            await Assert.That(() => (string)url!).Throws<InvalidOperationException>();
        }
    }
}
