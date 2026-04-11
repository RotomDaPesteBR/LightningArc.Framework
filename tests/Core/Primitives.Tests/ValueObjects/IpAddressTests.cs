using LightningArc.Primitives.ValueObjects;

namespace LightningArc.Primitives.Tests.ValueObjects
{
    public class IpAddressTests
    {
        [Test]
        public async Task Create_ValidIPv4_ShouldCreateInstance()
        {
            IpAddress ip = IpAddress.Create("192.168.1.1");

            await Assert.That(ip.Value).IsEqualTo("192.168.1.1");
            await Assert.That(ip.Version).IsEqualTo(4);
        }

        [Test]
        public async Task Create_ValidIPv6_ShouldCreateInstance()
        {
            IpAddress ip = IpAddress.Create("::1");

            await Assert.That(ip.Value).IsNotEmpty();
            await Assert.That(ip.Version).IsEqualTo(6);
        }

        [Test]
        public async Task Create_InvalidIp_ShouldThrowArgumentException()
        {
            await Assert.That(() => IpAddress.Create("not-an-ip")).Throws<ArgumentException>();
        }

        [Test]
        [Arguments("")] // Empty
        [Arguments("256.256.256.256")] // Invalid octets
        [Arguments("192.168.1.1.1")] // Extra octet
        [Arguments("abc.def.ghi.jkl")] // Letters
        public async Task Create_InvalidIPv4_ShouldThrowArgumentException(string invalidIp)
        {
            await Assert.That(() => IpAddress.Create(invalidIp)).Throws<ArgumentException>();
        }

        [Test]
        public async Task TryCreate_ValidIPv4_ShouldReturnTrue()
        {
            bool success = IpAddress.TryCreate("10.0.0.1", out IpAddress? ip);

            await Assert.That(success).IsTrue();
            await Assert.That(ip).IsNotNull();
            await Assert.That(ip!.Version).IsEqualTo(4);
        }

        [Test]
        public async Task TryCreate_ValidIPv6_ShouldReturnTrue()
        {
            bool success = IpAddress.TryCreate("2001:db8::1", out IpAddress? ip);

            await Assert.That(success).IsTrue();
            await Assert.That(ip).IsNotNull();
            await Assert.That(ip!.Version).IsEqualTo(6);
        }

        [Test]
        public async Task TryCreate_InvalidIp_ShouldReturnFalse()
        {
            bool success = IpAddress.TryCreate("invalid", out IpAddress? ip);

            await Assert.That(success).IsFalse();
            await Assert.That(ip is null).IsTrue();
        }

        [Test]
        public async Task ImplicitConversion_ToString_ShouldReturnValue()
        {
            IpAddress ip = IpAddress.Create("192.168.1.1");
            string result = ip;

            await Assert.That(result).IsEqualTo("192.168.1.1");
        }

        [Test]
        public async Task Create_FromString_ShouldCreateIp()
        {
            IpAddress ip = IpAddress.Create("192.168.1.1");

            await Assert.That(ip.Value).IsEqualTo("192.168.1.1");
            await Assert.That(ip.Version).IsEqualTo(4);
        }

        [Test]
        public async Task ToString_ShouldReturnValue()
        {
            IpAddress ip = IpAddress.Create("192.168.1.1");

            await Assert.That(ip.ToString()).IsEqualTo("192.168.1.1");
        }
    }
}
