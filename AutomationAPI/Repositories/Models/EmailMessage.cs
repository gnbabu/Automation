namespace AutomationAPI.Repositories.Models
{
    // Provider-agnostic email payload - every IEmailService implementation (the generic
    // SMTP provider, or any future provider) accepts the same shape, so callers never
    // need to know or care which provider is actually active.
    public class EmailMessage
    {
        public string To { get; set; } = string.Empty;
        public IEnumerable<string>? Cc { get; set; }
        public IEnumerable<string>? Bcc { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;

        // If not supplied, providers derive it from HtmlBody (strip tags) rather than
        // sending an HTML-only message with no plain-text alternative part.
        public string? PlainTextBody { get; set; }
    }
}
