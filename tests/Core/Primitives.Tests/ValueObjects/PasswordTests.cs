using LightningArc.Primitives.ValueObjects;

namespace LightningArc.Primitives.Tests.ValueObjects
{
    public class PasswordTests
    {
        [Test]
        public async Task Create_ValidPassword_ShouldCreateInstance()
        {
            // 12 chars (>=10), has $ (symbol) -> score=2 -> Moderate
            var password = Password.Create("Test$123Pass");

            await Assert.That(password.Value).IsEqualTo("Test$123Pass");
        }

        [Test]
        public async Task Create_PasswordWithMinimumStrength_ShouldCreateInstance()
        {
            // 9 chars has upper,lower,digit but no symbol -> score=0 -> Weak, allowed when min=Weak
            var password = Password.Create("Str0ngPass", minimumStrength: PasswordStrength.Weak);

            await Assert.That(password.Value).IsEqualTo("Str0ngPass");
        }

        [Test]
        public async Task Create_ToShortPassword_ShouldThrowArgumentException()
        {
            await Assert.That(() => Password.Create("short")).Throws<ArgumentException>();
        }

        [Test]
        public async Task Create_NoUppercasePassword_ShouldThrowArgumentException()
        {
            await Assert.That(() => Password.Create("password1abc")).Throws<ArgumentException>();
        }

        [Test]
        public async Task Create_NoDigitPassword_ShouldThrowArgumentException()
        {
            await Assert.That(() => Password.Create("PassworDabc")).Throws<ArgumentException>();
        }

        [Test]
        public async Task TryCreate_ValidPassword_ShouldReturnTrue()
        {
            var success = Password.TryCreate("Test$123Pass", out var password);

            await Assert.That(success).IsTrue();
            await Assert.That(password).IsNotNull();
            await Assert.That(password!.Value).IsEqualTo("Test$123Pass");
        }

        [Test]
        public async Task TryCreate_WeakPassword_ShouldReturnFalse()
        {
            var success = Password.TryCreate("short", out var password);

            await Assert.That(success).IsFalse();
            await Assert.That(password is null).IsTrue();
        }

        [Test]
        public async Task ToString_ShouldReturnMaskedPassword()
        {
            var password = Password.Create("Test$123Pass");

            await Assert.That(password.ToString()).IsEqualTo("************");
        }

        [Test]
        public async Task Strength_WeakPassword_WhenAllowed_ShouldCreateInstance()
        {
            // 8 chars, has upper,lower,digit but no symbol -> score=0 -> Weak, allowed
            var password = Password.Create("Pass1abc", minimumStrength: PasswordStrength.Weak);

            await Assert.That(password.Value).IsEqualTo("Pass1abc");
        }

        [Test]
        public async Task Strength_StrongPassword_ShouldReturnStrongStrength()
        {
            // score >= 4 requires: length >= 10 + has symbol + length >= 14 + length >= 18 = 4
            // "Test$12Password!9a" - 18 chars (>=10, >=14, >=18), no actual symbol character
            // Let me use: "Pass$word12345678" - 17 chars, has $ -> score=3 -> not strong
            // Need 20 chars with $: "Test$12PassWordAb890"
            var password = Password.Create("Test$12PassWordAb890");

            await Assert.That(password.Strength).IsEqualTo(PasswordStrength.Strong);
        }
    }
}
