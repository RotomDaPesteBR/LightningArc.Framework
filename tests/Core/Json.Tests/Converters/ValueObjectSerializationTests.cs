using System.Text.Json;
using LightningArc.Primitives.ValueObjects;
using LightningArc.Json.Converters;

namespace LightningArc.Json.Tests.Converters;

public class ValueObjectSerializationTests
{
    private readonly JsonSerializerOptions _options;

    public ValueObjectSerializationTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new ValueObjectJsonConverterFactory());
    }

    [Test]
    public async Task Email_ShouldSerializeToPlainString()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        string json = JsonSerializer.Serialize(email, _options);

        // Assert
        await Assert.That(json).IsEqualTo("\"test@example.com\"");
    }

    [Test]
    public async Task Email_ShouldDeserializeFromPlainString()
    {
        // Arrange
        string json = "\"test@example.com\"";

        // Act
        var email = JsonSerializer.Deserialize<Email>(json, _options);

        // Assert
        await Assert.That(email).IsNotNull();
        await Assert.That(email!.Value).IsEqualTo("test@example.com");
    }

    [Test]
    public async Task Cpf_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var cpf = Cpf.Create("12345678909");
        string json = JsonSerializer.Serialize(cpf, _options);

        // Act
        var deserialized = JsonSerializer.Deserialize<Cpf>(json, _options);

        // Assert
        await Assert.That(json).IsEqualTo("\"12345678909\"");
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!).IsEqualTo(cpf);
    }

    [Test]
    public async Task Url_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var url = Url.Create("https://google.com");
        string json = JsonSerializer.Serialize(url, _options);

        // Act
        var deserialized = JsonSerializer.Deserialize<Url>(json, _options);

        // Assert
        await Assert.That(json).IsEqualTo("\"https://google.com\"");
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!).IsEqualTo(url);
    }

    [Test]
    public async Task Deserialize_InvalidValue_ShouldThrowJsonException()
    {
        // Arrange
        string json = "\"invalid-email\"";

        // Act & Assert
        try
        {
            JsonSerializer.Deserialize<Email>(json, _options);
            Assert.Fail("Should have thrown a JsonException");
        }
        catch (JsonException ex)
        {
            await Assert.That(ex.Message).Contains("não é um endereço de e-mail válido");
        }
    }
}
