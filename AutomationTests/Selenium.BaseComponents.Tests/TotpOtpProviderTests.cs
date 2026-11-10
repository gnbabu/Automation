using System.Text;
using NUnit.Framework;
using Selenium.BaseComponents.Ohid;

namespace Selenium.BaseComponents.Tests
{
    [TestFixture]
    public class TotpOtpProviderTests
    {
        private const string Rfc6238Base32 = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ";
        private static readonly byte[] Rfc6238Key = Encoding.ASCII.GetBytes("12345678901234567890");

        [TestCase(59L, "94287082")]
        [TestCase(1111111109L, "07081804")]
        [TestCase(1234567890L, "89005924")]
        [TestCase(2000000000L, "69279037")]
        public void MatchesRfc6238Sha1Vectors(long unixTime, string expected)
        {
            Assert.That(TotpOtpProvider.GenerateCode(Rfc6238Key, unixTime / 30, 8), Is.EqualTo(expected));
        }

        [TestCase(59L, "287082")]
        [TestCase(1234567890L, "005924")]
        public void SixDigitCodesForTheKeyText(long unixTime, string expected)
        {
            var provider = new TotpOtpProvider(Rfc6238Base32);
            Assert.That(provider.GenerateCode(DateTimeOffset.FromUnixTimeSeconds(unixTime)), Is.EqualTo(expected));
        }

        [Test]
        public void Base32IgnoresSpacesAndCase()
        {
            Assert.That(TotpOtpProvider.Base32Decode("gezd gnbv gy3t qojq gezd gnbv gy3t qojq"), Is.EqualTo(Rfc6238Key));
        }

        [Test]
        public void Base32ReadsZeroAsLetterO()
        {
            Assert.That(TotpOtpProvider.Base32Decode("ABC0"), Is.EqualTo(TotpOtpProvider.Base32Decode("ABCO")));
        }

        [Test]
        public void Base32RejectsInvalidCharacters()
        {
            Assert.Throws<FormatException>(() => TotpOtpProvider.Base32Decode("ABC8"));
            Assert.Throws<FormatException>(() => TotpOtpProvider.Base32Decode("   "));
        }

        [Test]
        public async Task ReturnsCurrentCodeWhenEnoughTimeIsLeft()
        {
            var now = DateTimeOffset.FromUnixTimeSeconds(1234567890 - 1234567890 % 30 + 3);
            var provider = new TotpOtpProvider(Rfc6238Base32, () => now);

            Assert.That(await provider.WaitForOtpAsync(TimeSpan.FromSeconds(5)), Is.EqualTo(provider.GenerateCode(now)));
        }

        [Test]
        public async Task WaitsForNextWindowNearTheEdge()
        {
            var now = DateTimeOffset.FromUnixTimeSeconds(1234567890 - 1234567890 % 30 + 28);
            var provider = new TotpOtpProvider(Rfc6238Base32, () => now);

            Assert.That(await provider.WaitForOtpAsync(TimeSpan.FromSeconds(1)), Is.Null);
        }

        [Test]
        public void ExtractsSixToEightDigits()
        {
            var provider = new TotpOtpProvider(Rfc6238Base32);
            Assert.That(provider.ExtractOtp("Your code is 482913."), Is.EqualTo("482913"));
            Assert.That(provider.ExtractOtp("Call 800-686-1516"), Is.Null);
        }
    }

    [TestFixture]
    public class OhidSettingsTests
    {
        private string _dir = null!;

        [SetUp]
        public void CreateFolder() => _dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "ohid-settings-" + Guid.NewGuid())).FullName;

        [TearDown]
        public void DeleteFolder() => Directory.Delete(_dir, true);

        [Test]
        public void LocalFileWinsOverPlaceholder()
        {
            File.WriteAllText(Path.Combine(_dir, "appSettings.json"), "{ \"Otp\": { \"TotpKey\": \"\" } }");
            File.WriteAllText(Path.Combine(_dir, OhidSettings.LocalFileName), "{ \"Otp\": { \"TotpKey\": \"GEZDGNBVGY3TQOJQ\" } }");

            Assert.That(OhidSettings.GetTotpKey(_dir), Is.EqualTo("GEZDGNBVGY3TQOJQ"));
        }

        [Test]
        public void MissingKeyGivesClearError()
        {
            File.WriteAllText(Path.Combine(_dir, "appSettings.json"), "{ \"Otp\": { \"TotpKey\": \"\" } }");

            var ex = Assert.Throws<InvalidOperationException>(() => OhidSettings.GetTotpKey(_dir));
            Assert.That(ex!.Message, Does.Contain(OhidSettings.LocalFileName));
        }
    }
}
