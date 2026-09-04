using System.Text.RegularExpressions;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AutomationAPI.Repositories
{
    // Generic, vendor-agnostic SMTP email sender - not tied to any specific provider.
    // Brevo is configured through this today; any future SMTP-based provider (Mailgun,
    // Amazon SES's SMTP interface, Office365, Postmark in SMTP mode, Zoho, etc.) needs
    // zero new code, just another named SmtpProviderSettings config block + one more
    // keyed DI registration in Program.cs pointed at this same class.
    //
    // Uses MailKit (not System.Net.Mail.SmtpClient) - confirmed via direct testing against
    // the real Brevo relay that .NET's legacy SmtpClient has a longstanding STARTTLS/AUTH
    // negotiation bug against modern relays ("5.7.0 Please authenticate first" - the AUTH
    // command was never actually issued before MAIL FROM). This is a well-documented
    // limitation of System.Net.Mail.SmtpClient (Microsoft itself recommends MailKit for
    // anything beyond the most trivial scenarios); MailKit is the de facto standard,
    // actively-maintained SMTP client for .NET specifically because of this class of bug.
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpProviderSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;

        // Purely for clear log messages (e.g. "Brevo") - matches the key this instance
        // was registered under in Program.cs.
        private readonly string _providerName;

        public SmtpEmailService(SmtpProviderSettings settings, ILogger<SmtpEmailService> logger, string providerName)
        {
            _settings = settings;
            _logger = logger;
            _providerName = providerName;
        }

        public Task SendAsync(string to, string subject, string body)
        {
            return SendAsync(new EmailMessage { To = to, Subject = subject, HtmlBody = body });
        }

        public async Task SendAsync(EmailMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.To))
                throw new ArgumentException("Recipient email is required");

            if (string.IsNullOrWhiteSpace(message.Subject))
                throw new ArgumentException("Email subject is required");

            if (string.IsNullOrWhiteSpace(message.HtmlBody))
                throw new ArgumentException("Email body is required");

            if (!_settings.Enabled)
                throw new Exception($"{_providerName} email provider is not enabled");

            if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
                throw new Exception($"{_providerName} SmtpHost not configured");

            if (string.IsNullOrWhiteSpace(_settings.Username) || string.IsNullOrWhiteSpace(_settings.Password))
                throw new Exception($"{_providerName} SMTP credentials not configured");

            if (string.IsNullOrWhiteSpace(_settings.FromEmail))
                throw new Exception($"{_providerName} FromEmail not configured");

            var plainText = message.PlainTextBody
                ?? Regex.Replace(message.HtmlBody, "<.*?>", string.Empty);

            var mime = new MimeMessage();
            mime.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            mime.To.Add(MailboxAddress.Parse(message.To));
            foreach (var cc in message.Cc ?? Enumerable.Empty<string>())
                mime.Cc.Add(MailboxAddress.Parse(cc));
            foreach (var bcc in message.Bcc ?? Enumerable.Empty<string>())
                mime.Bcc.Add(MailboxAddress.Parse(bcc));
            mime.Subject = message.Subject;

            mime.Body = new BodyBuilder
            {
                HtmlBody = message.HtmlBody,
                TextBody = plainText
            }.ToMessageBody();

            using var client = new SmtpClient();

            try
            {
                await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(mime);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Never log Username/Password - only the operation/recipient/subject/
                // provider and the exception's own message.
                _logger.LogError(ex,
                    "Email send failed. Operation={Operation} Provider={Provider} Recipient={Recipient} Subject={Subject}",
                    "SendEmail", _providerName, message.To, message.Subject);
                throw new Exception($"{_providerName} SMTP send failed: {ex.Message}", ex);
            }
        }
    }
}
