using LightningArc.Primitives.ValueObjects;

namespace LightningArc.Primitives.Tests.ValueObjects
{
    public class RgTests
    {
        [Test]
        public async Task Create_ValidRgFormatted_ShouldCreateInstance()
        {
            const string rgValue = "12.345.678-9";
            Rg rg = Rg.Create(rgValue);

            await Assert.That(rg.Value).IsNotEmpty();
        }

        [Test]
        public async Task Create_ValidRgUnformatted_ShouldCreateInstance()
        {
            const string rgValue = "123456789";
            Rg rg = Rg.Create(rgValue);

            await Assert.That(rg.Value).IsNotEmpty();
        }

        [Test]
        public async Task Create_ValidRgWithX_ShouldCreateInstance()
        {
            const string rgValue = "12.345.678-X";
            Rg rg = Rg.Create(rgValue);

            await Assert.That(rg.Value).Contains("X");
        }

        [Test]
        [Arguments("123")] // Too short
        [Arguments("abc.def.ghi-j")] // Letters
        [Arguments("")] // Empty
        [Arguments("abcdefghijklmnop")] // All invalid characters
        public async Task Create_InvalidRg_ShouldThrowArgumentException(string invalidRg)
        {
            await Assert.That(() => Rg.Create(invalidRg)).Throws<ArgumentException>();
        }

        [Test]
        public async Task TryCreate_ValidRg_ShouldReturnTrue()
        {
            bool success = Rg.TryCreate("12.345.678-9", out Rg? rg);

            await Assert.That(success).IsTrue();
            await Assert.That(rg).IsNotNull();
            await Assert.That(rg!.Value).IsNotEmpty();
        }

        [Test]
        public async Task TryCreate_InvalidRg_ShouldReturnFalse()
        {
            bool success = Rg.TryCreate("invalid", out Rg? rg);

            await Assert.That(success).IsFalse();
            await Assert.That(rg is null).IsTrue();
        }

        [Test]
        public async Task ImplicitConversion_ToString_ShouldReturnStringValue()
        {
            Rg rg = Rg.Create("12.345.678-9");
            string result = rg;

            await Assert.That(result).IsEqualTo(rg.Value);
        }

        [Test]
        public async Task ImplicitConversion_FromString_ShouldCreateRg()
        {
            Rg rg = "123456789";

            await Assert.That(rg.Value).IsNotEmpty();
        }

        [Test]
        public async Task ToString_ShouldReturnValue()
        {
            Rg rg = Rg.Create("12.345.678-9");

            await Assert.That(rg.ToString()).IsEqualTo(rg.Value);
        }
    }
}
