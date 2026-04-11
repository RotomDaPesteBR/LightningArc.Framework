using LightningArc.Primitives.ValueObjects;

namespace LightningArc.Primitives.Tests.ValueObjects
{
    public class CurrencyTests
    {
        [Test]
        public async Task Create_ValidCurrency_ShouldCreateInstance()
        {
            Currency currency = Currency.Create(99.90m, "BRL");

            await Assert.That(currency.Value).IsEqualTo(99.90m);
            await Assert.That(currency.Code).IsEqualTo("BRL");
        }

        [Test]
        public async Task Create_LowercaseCode_ShouldNormalizeToUpper()
        {
            Currency currency = Currency.Create(19.99m, "usd");

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
            bool success = Currency.TryCreate(50.00m, "EUR", out Currency? currency);

            await Assert.That(success).IsTrue();
            await Assert.That(currency).IsNotNull();
            await Assert.That(currency!.Value).IsEqualTo(50.00m);
            await Assert.That(currency.Code).IsEqualTo("EUR");
        }

        [Test]
        public async Task TryCreate_InvalidCurrency_ShouldReturnFalse()
        {
            bool success = Currency.TryCreate(0m, "XXX", out Currency? currency);

            await Assert.That(success).IsFalse();
            await Assert.That(currency is null).IsTrue();
        }

        [Test]
        [Arguments(100, "JPY")]
        [Arguments(10.50, "BRL")]
        [Arguments(1.234, "KWD")]
        public async Task Create_ValidPrecision_ShouldCreateInstance(decimal value, string code)
        {
            Currency currency = Currency.Create(value, code);
            await Assert.That(currency.Value).IsEqualTo(value);
        }

        [Test]
        [Arguments(100.50, "JPY", "at most 0 decimal places")]
        [Arguments(10.123, "BRL", "at most 2 decimal places")]
        [Arguments(1.1234, "KWD", "at most 3 decimal places")]
        public async Task Create_InvalidPrecision_ShouldThrowArgumentException(decimal value, string code, string expectedMessage)
        {
            ArgumentException? exception = await Assert.That((Func<Currency>)Action).Throws<ArgumentException>();
            await Assert.That(exception?.Message).Contains(expectedMessage);
            return;
            Currency Action() => Currency.Create(value, code);
        }

        [Test]
        public async Task TryCreate_InvalidPrecision_ShouldReturnFalse()
        {
            bool success = Currency.TryCreate(100.50m, "JPY", out Currency? currency);
            await Assert.That(success).IsFalse();
            await Assert.That(currency).IsNull();
        }

        [Test]
        public async Task ImplicitConversion_ToDecimal_ShouldReturnValue()
        {
            Currency currency = Currency.Create(123.45m, "BRL");
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
            Currency currency = Currency.Create(99.90m, "BRL");

            await Assert.That(currency.ToString()).Contains("BRL");
        }
    }
}
