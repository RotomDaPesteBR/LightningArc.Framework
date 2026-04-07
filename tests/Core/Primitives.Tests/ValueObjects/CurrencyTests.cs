using LightningArc.Primitives.ValueObjects;

namespace LightningArc.Primitives.Tests.ValueObjects
{
    public class CurrencyTests
    {
        [Test]
        public async Task Create_ValidCurrency_ShouldCreateInstance()
        {
            var currency = Currency.Create(99.90m, "BRL");

            await Assert.That(currency.Value).IsEqualTo(99.90m);
            await Assert.That(currency.Code).IsEqualTo("BRL");
        }

        [Test]
        public async Task Create_LowercaseCode_ShouldNormalizeToUpper()
        {
            var currency = Currency.Create(19.99m, "usd");

            await Assert.That(currency.Code).IsEqualTo("USD");
        }

        [Test]
        public async Task Create_InvalidCode_ShouldThrowArgumentException()
        {
            await Assert.That(() => Currency.Create(0m, "")).Throws<ArgumentException>();
            await Assert.That(() => Currency.Create(0m, "AB")).Throws<ArgumentException>();
            await Assert.That(() => Currency.Create(0m, "ABCD")).Throws<ArgumentException>();
            await Assert.That(() => Currency.Create(0m, "XYZ")).Throws<ArgumentException>();
            await Assert.That(() => Currency.Create(0m, "B1L")).Throws<ArgumentException>();
        }

        [Test]
        public async Task TryCreate_ValidCurrency_ShouldReturnTrue()
        {
            var success = Currency.TryCreate(50.00m, "EUR", out var currency);

            await Assert.That(success).IsTrue();
            await Assert.That(currency).IsNotNull();
            await Assert.That(currency!.Value).IsEqualTo(50.00m);
            await Assert.That(currency.Code).IsEqualTo("EUR");
        }

        [Test]
        public async Task TryCreate_InvalidCurrency_ShouldReturnFalse()
        {
            var success = Currency.TryCreate(0m, "XXX", out var currency);

            await Assert.That(success).IsFalse();
            await Assert.That(currency is null).IsTrue();
        }

        [Test]
        public async Task ImplicitConversion_ToDecimal_ShouldReturnValue()
        {
            var currency = Currency.Create(123.45m, "BRL");
            decimal result = currency;

            await Assert.That(result).IsEqualTo(123.45m);
        }

        [Test]
        public async Task ImplicitConversion_NullToDecimal_ShouldReturnZero()
        {
            Currency? currency = null;
            decimal result = currency!;

            await Assert.That(result).IsEqualTo(0m);
        }

        [Test]
        public async Task ToString_ShouldReturnFormattedCurrency()
        {
            var currency = Currency.Create(99.90m, "BRL");

            await Assert.That(currency.ToString()).Contains("BRL");
        }
    }
}
