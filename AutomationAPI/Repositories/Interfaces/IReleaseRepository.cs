using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IReleaseRepository
    {
        Task<int> CreateAsync(ReleaseRequestDto releaseRequest);
        Task SetFolderPathAsync(int releaseId, string releaseFolderPath);
        Task DeleteAsync(int releaseId);
        Task UpdateAsync(ReleaseRequestDto releaseRequest);
        Task<IEnumerable<ReleaseModel>> GetAllAsync();
        Task<ReleaseModel> GetByIdAsync(int releaseId);
        Task ActivateAsync(int releaseId, string activatedBy);
        Task SignOffAsync(int releaseId, ReleaseSignOffRequestDto request);
        Task<IEnumerable<ReleaseSignOffModel>> GetSignOffHistoryAsync(int releaseId);

        // Notifications
        Task<int> AddNotificationAsync(int releaseId, string notificationType, int? recipientUserId, string recipientEmail, string message);
        Task<IEnumerable<ReleaseNotificationModel>> GetNotificationsAsync(int releaseId);
        Task MarkNotificationAsync(int releaseNotificationId, string status);
    }
}
