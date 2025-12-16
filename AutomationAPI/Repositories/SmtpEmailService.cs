using System.Net.Mail;
using System.Net;
using AutomationAPI.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public SmtpEmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var message = new MailMessage
            {
                From = new MailAddress(
                    _settings.SenderEmail,
                    _settings.SenderName
                ),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(to);

            using var smtp = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                Credentials = new NetworkCredential(
                    _settings.Username,
                    _settings.Password
                ),
                EnableSsl = _settings.EnableSsl
            };

            await smtp.SendMailAsync(message);
        }
    }
}
