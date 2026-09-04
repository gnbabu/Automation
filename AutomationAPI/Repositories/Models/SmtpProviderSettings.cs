namespace AutomationAPI.Repositories.Models
{
    // Generic settings shape for any standard-SMTP-based email provider (Brevo, Mailgun,
    // Amazon SES's SMTP interface, Office365, Postmark in SMTP mode, Zoho, etc.) - the
    // protocol is identical across all of them (RFC 5321 SMTP over TLS), only these
    // values differ, so one settings type + one SmtpEmailService implementation serves
    // every one of them. Bound via .NET's *named* options (Configure<T>(name, section)),
    // one named instance per provider (e.g. "Brevo"), not a single fixed config section.
    public class SmtpProviderSettings
    {
        public bool Enabled { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }
}
