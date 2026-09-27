using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IReleaseNotificationService
    {
        Task<ReleaseNotifyResult> NotifyManagersAndAdminsAsync(
            int releaseId,
            string notificationType,
            string subject,
            string bodyHtml);
    }
}
