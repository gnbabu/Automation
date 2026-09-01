using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    /// <summary>
    /// Sends a Release-related notification to active Manager/Admin users, recording
    /// each attempt in aut.ReleaseNotification. Shared by ReleaseController.Activate
    /// (notificationType "ActivatedForTesting") and the background
    /// ReleaseDllsReadyNotificationWorker (notificationType "DllsReadyForActivation"),
    /// so the recipient-resolution logic lives in exactly one place.
    /// </summary>
    public interface IReleaseNotificationService
    {
        Task<ReleaseNotifyResult> NotifyManagersAndAdminsAsync(
            int releaseId,
            string notificationType,
            string subject,
            string bodyHtml);
    }
}
