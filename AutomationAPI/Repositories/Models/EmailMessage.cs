namespace AutomationAPI.Repositories.Models
{
    public class EmailMessage
    {
        public string To { get; set; } = string.Empty;
        public IEnumerable<string>? Cc { get; set; }
        public IEnumerable<string>? Bcc { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
        public string? PlainTextBody { get; set; }
    }
}
