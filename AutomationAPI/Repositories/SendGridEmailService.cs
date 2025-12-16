using System.Text.RegularExpressions;
using AutomationAPI.Repositories.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
namespace AutomationAPI.Repositories
{
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SendGridEmailService(IConfiguration config)
        {
            _config = config;
        }



        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient email is required");

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Email subject is required");

            if (string.IsNullOrWhiteSpace(htmlBody))
                throw new ArgumentException("Email body is required");

            var apiKey = _config["SendGrid:ApiKey"];
            var fromEmail = _config["SendGrid:FromEmail"];
            var fromName = _config["SendGrid:FromName"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("SendGrid API key not configured");

            if (string.IsNullOrWhiteSpace(fromEmail))
                throw new Exception("SendGrid FromEmail not configured");

            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(fromEmail, fromName);
            var toEmail = new EmailAddress(to);

            // ✅ Generate plain text automatically from HTML (no hard-coding)
            var plainText = Regex.Replace(htmlBody, "<.*?>", string.Empty);

            var msg = MailHelper.CreateSingleEmail(
                from,
                toEmail,
                subject,
                plainTextContent: plainText,
                htmlContent: htmlBody
            );

            // ✅ Required to avoid SendGrid errors
            msg.SetReplyTo(new EmailAddress(fromEmail, fromName));

            // ✅ Reduce spam score
            msg.SetClickTracking(false, false);
            msg.AddCategory("OHPNM-Automation");

            var response = await client.SendEmailAsync(msg);

            // Optional: validate SendGrid response
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid failed: {response.StatusCode} - {error}");
            }
        }

    }
}
