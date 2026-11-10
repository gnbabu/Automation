using System.Text.RegularExpressions;

namespace Selenium.BaseComponents.Ohid
{
    public interface IOtpProvider
    {
        Task<string?> GetLatestOtpAsync();
        Task<string?> WaitForOtpAsync(TimeSpan timeout);
        string? ExtractOtp(string? message);
    }

    public static class OtpText
    {
        private static readonly Regex CodePattern = new(@"\b\d{6,8}\b");

        public static string? Extract(string? message)
        {
            if (string.IsNullOrWhiteSpace(message)) return null;
            var match = CodePattern.Match(message);
            return match.Success ? match.Value : null;
        }
    }
}
