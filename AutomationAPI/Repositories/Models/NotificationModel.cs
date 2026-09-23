namespace AutomationAPI.Repositories.Models
{
    public class NotificationModel
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string NotificationType { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string LinkUrl { get; set; }
        public string SourceType { get; set; }
        public int? SourceId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
