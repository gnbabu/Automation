using System.Security.Cryptography;

namespace Selenium.BaseComponents.Ohid
{
    public class TotpOtpProvider : IOtpProvider
    {
        public const int StepSeconds = 30;
        private const int Digits = 6;
        private static readonly TimeSpan MinimumRemaining = TimeSpan.FromSeconds(5);

        private readonly byte[] _key;
        private readonly Func<DateTimeOffset> _clock;
        private long _lastUsedStep = -1;

        public TotpOtpProvider(string base32Key, Func<DateTimeOffset>? clock = null)
        {
            _key = Base32Decode(base32Key);
            _clock = clock ?? (() => DateTimeOffset.UtcNow);
        }

        public Task<string?> GetLatestOtpAsync() => Task.FromResult<string?>(GenerateCode(_clock()));

        public async Task<string?> WaitForOtpAsync(TimeSpan timeout)
        {
            var now = _clock();
            var step = Step(now);
            var wait = step == _lastUsedStep || TimeLeftInStep(now) < MinimumRemaining
                ? TimeLeftInStep(now) + TimeSpan.FromMilliseconds(250)
                : TimeSpan.Zero;

            if (wait > timeout) return null;
            if (wait > TimeSpan.Zero) await Task.Delay(wait);

            now = _clock();
            _lastUsedStep = Step(now);
            return GenerateCode(now);
        }

        public string? ExtractOtp(string? message) => OtpText.Extract(message);

        public string GenerateCode(DateTimeOffset time) => GenerateCode(_key, Step(time), Digits);

        public static string GenerateCode(byte[] key, long step, int digits)
        {
            var counter = BitConverter.GetBytes(step);
            if (BitConverter.IsLittleEndian) Array.Reverse(counter);

            using var hmac = new HMACSHA1(key);
            var hash = hmac.ComputeHash(counter);
            var offset = hash[^1] & 0x0F;
            var binary = ((hash[offset] & 0x7F) << 24)
                       | (hash[offset + 1] << 16)
                       | (hash[offset + 2] << 8)
                       | hash[offset + 3];

            return (binary % (int)Math.Pow(10, digits)).ToString().PadLeft(digits, '0');
        }

        public static byte[] Base32Decode(string base32)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var cleaned = new string((base32 ?? "").Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray())
                .TrimEnd('=').ToUpperInvariant()
                .Replace('0', 'O')
                .Replace('1', 'I');
            if (cleaned.Length == 0)
                throw new FormatException("The authenticator key is empty.");

            var bytes = new List<byte>(cleaned.Length * 5 / 8);
            int buffer = 0, bitsLeft = 0;
            foreach (var c in cleaned)
            {
                var value = alphabet.IndexOf(c);
                if (value < 0)
                    throw new FormatException("The authenticator key isn't valid Base32 (allowed: A–Z and 2–7).");

                buffer = (buffer << 5) | value;
                bitsLeft += 5;
                if (bitsLeft >= 8)
                {
                    bytes.Add((byte)(buffer >> (bitsLeft - 8)));
                    bitsLeft -= 8;
                }
            }

            return bytes.ToArray();
        }

        private static long Step(DateTimeOffset time) => time.ToUnixTimeSeconds() / StepSeconds;

        private static TimeSpan TimeLeftInStep(DateTimeOffset time)
        {
            var stepEnd = DateTimeOffset.FromUnixTimeSeconds((Step(time) + 1) * StepSeconds);
            return stepEnd - time;
        }
    }
}
