using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Selenium.BaseComponents.Ohid
{
    public static class OhidSettings
    {
        public const string LocalFileName = "appSettings.Local.json";
        public const string TotpKeySetting = "Otp:TotpKey";

        public const string PnmUserIdSetting = "Otp:PnmUserId";

        public static string? GetPnmUserId(string? directory = null)
        {
            var value = Load(directory)[PnmUserIdSetting];
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public static string GetTotpKey(string? directory = null)
        {
            var key = Load(directory)[TotpKeySetting];
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException(
                    $"OH|ID TOTP key not configured: add \"Otp\": {{ \"TotpKey\": \"...\" }} to {LocalFileName} next to the test DLLs.");

            TotpOtpProvider.Base32Decode(key);
            return key;
        }

        private static IConfiguration Load(string? directory)
        {
            directory ??= Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();

            return new ConfigurationBuilder()
                .SetBasePath(directory)
                .AddJsonFile("appSettings.json", optional: true)
                .AddJsonFile(LocalFileName, optional: true)
                .Build();
        }
    }
}
