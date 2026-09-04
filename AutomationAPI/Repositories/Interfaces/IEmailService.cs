using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);

        // Richer send - Cc/Bcc/plain-text support. Every implementation should have the
        // simple 3-arg overload above delegate into this one internally, rather than
        // duplicating validation/send/logging logic between the two.
        Task SendAsync(EmailMessage message);
    }
}
