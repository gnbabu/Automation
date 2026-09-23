using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<int> AddAsync(int userId, string notificationType, string title, string message, string linkUrl, string sourceType, int? sourceId);
        Task<IEnumerable<NotificationModel>> GetByUserAsync(int userId, bool unreadOnly);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkReadAsync(int notificationId, int userId);
        Task MarkAllReadAsync(int userId);
    }
}
