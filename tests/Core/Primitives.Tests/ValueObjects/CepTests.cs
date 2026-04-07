using LightningArc.Primitives.ValueObjects;

namespace LightningArc.Primitives.Tests.ValueObjects
{
    public class CepTests
    {
        [Test]
        public async Task Create_ValidCepWithHyphen_ShouldCreateInstance()
        {
            const string cepValue = "01001-000";
            var cep = Cep.Create(cepValue);

            await Assert.That(cep.Value).IsEqualTo("01001-000");
        }

        [Test]
        public async Task Create_ValidCepWithoutHyphen_ShouldNormalizeAndCreate()
        {
            const string cepValue = "01001000";
            var cep = Cep.Create(cepValue);

            await Assert.That(cep.Value).IsEqualTo("01001-000");
        }

        [Test]
        [Arguments("0100100")] // Too short
        [Arguments("010010000")] // Too long
        [Arguments("01001ABC")] // Non-numeric
        [Arguments("")] // Empty
        [Arguments("01001-00A")] // Invalid character
        public async Task Create_InvalidCep_ShouldThrowArgumentException(string invalidCep)
        {
            await Assert.That(() => Cep.Create(invalidCep)).Throws<ArgumentException>();
        }

        [Test]
        public async Task TryCreate_ValidCep_ShouldReturnTrue()
        {
            const string cepValue = "01001-000";

            var success = Cep.TryCreate(cepValue, out var cep);

            await Assert.That(success).IsTrue();
            await Assert.That(cep).IsNotNull();
            await Assert.That(cep!.Value).IsEqualTo("01001-000");
        }

        [Test]
        public async Task TryCreate_InvalidCep_ShouldReturnFalse()
        {
            var success = Cep.TryCreate("invalid", out var cep);

            await Assert.That(success).IsFalse();
            await Assert.That(cep is null).IsTrue();
        }

        [Test]
        public async Task ImplicitConversion_ToString_ShouldReturnStringValue()
        {
            var cep = Cep.Create("01001-000");
            string result = cep;

            await Assert.That(result).IsEqualTo("01001-000");
        }

        [Test]
        public async Task ImplicitConversion_FromString_ShouldCreateCep()
        {
            Cep cep = "01001000";

            await Assert.That(cep.Value).IsEqualTo("01001-000");
        }

        [Test]
        public async Task ToString_ShouldReturnFormattedCep()
        {
            var cep = Cep.Create("01001000");

            await Assert.That(cep.ToString()).IsEqualTo("01001-000");
        }
    }
}
