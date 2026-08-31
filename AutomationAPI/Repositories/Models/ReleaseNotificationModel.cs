namespace AutomationAPI.Repositories.Models
{
    public class ReleaseNotificationModel
    {
        public int ReleaseNotificationId { get; set; }
        public int ReleaseId { get; set; }
        public string NotificationType { get; set; }
        public int? RecipientUserId { get; set; }
        public string RecipientEmail { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? SentOn { get; set; }
    }
}
