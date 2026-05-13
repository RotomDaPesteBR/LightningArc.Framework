
using LightningArc.Results;
using LightningArc.Results.Localization;
using LightningArc.Results.Successes;
using System;

namespace LightningArc.Results.Tests
{
    public class SuccessTests
    {
        [Test]
        public async Task Ok_ShouldCreateOkSuccess_WithDefaultMessage()
        {
            LocalizationManager.Configure("pt-BR");

            // Act
            Success success = Success.Ok();

            // Assert
            await Assert.That(success).IsTypeOf<OkSuccess>();
            await Assert.That(success.Code).IsEqualTo(100);
            await Assert.That(success.Message).IsEqualTo("Operação concluída com sucesso.");
        }

        [Test]
        public async Task Ok_ShouldCreateOkSuccess_WithCustomMessage()
        {
            // Arrange
            const string message = "Custom OK message";

            // Act
            Success success = Success.Ok(message);

            // Assert
            await Assert.That(success).IsTypeOf<OkSuccess>();
            await Assert.That(success.Code).IsEqualTo(100);
            await Assert.That(success.Message).IsEqualTo(message);
        }

        [Test]
        public async Task Created_ShouldCreateCreatedSuccess_WithDefaultMessage()
        {
            LocalizationManager.Configure("pt-BR");

            // Act
            Success success = Success.Created();

            // Assert
            await Assert.That(success).IsTypeOf<CreatedSuccess>();
            await Assert.That(success.Code).IsEqualTo(101);
            await Assert.That(success.Message).IsEqualTo("Recurso criado com sucesso.");
        }

        [Test]
        public async Task Created_ShouldCreateCreatedSuccess_WithCustomMessage()
        {
            // Arrange
            const string message = "Item was created";

            // Act
            Success success = Success.Created(message);

            // Assert
            await Assert.That(success).IsTypeOf<CreatedSuccess>();
            await Assert.That(success.Code).IsEqualTo(101);
            await Assert.That(success.Message).IsEqualTo(message);
        }

        [Test]
        public async Task Accepted_ShouldCreateAcceptedSuccess_WithDefaultMessage()
        {
            LocalizationManager.Configure("pt-BR");

            // Act
            Success success = Success.Accepted();
            
            // Assert
            await Assert.That(success).IsTypeOf<AcceptedSuccess>();
            await Assert.That(success.Code).IsEqualTo(102);
            await Assert.That(success.Message).IsEqualTo("A solicitação foi aceita para processamento.");
        }

        [Test]
        public async Task Accepted_ShouldCreateAcceptedSuccess_WithCustomMessage()
        {
            // Arrange
            const string message = "Request accepted";

            // Act
            Success success = Success.Accepted(message);

            // Assert
            await Assert.That(success).IsTypeOf<AcceptedSuccess>();
            await Assert.That(success.Code).IsEqualTo(102);
            await Assert.That(success.Message).IsEqualTo(message);
        }

        [Test]
        public async Task NoContent_ShouldCreateNoContentSuccess_WithNullMessage()
        {
            LocalizationManager.Configure("pt-BR");

            // Act
            Success success = Success.NoContent();

            // Assert
            await Assert.That(success).IsTypeOf<NoContentSuccess>();
            await Assert.That(success.Code).IsEqualTo(103);
        }

        [Test]
        public async Task NoContent_ShouldCreateNoContentSuccess_WithCustomMessage()
        {
            // Arrange
            const string message = "Should be null, but testing anyway";

            // Act
            Success success = Success.NoContent(message);

            // Assert
            await Assert.That(success).IsTypeOf<NoContentSuccess>();
            await Assert.That(success.Code).IsEqualTo(103);
            await Assert.That(success.Message).IsEqualTo(message);
        }
    }
}


