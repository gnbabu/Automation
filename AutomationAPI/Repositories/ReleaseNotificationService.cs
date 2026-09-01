using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories
{
    public class ReleaseNotificationService : IReleaseNotificationService
    {
        private readonly IReleaseRepository _releaseRepo;
        private readonly IUserRepository _userRepo;
        private readonly IEmailService _emailService;
        private readonly ILogger<ReleaseNotificationService> _logger;

        public ReleaseNotificationService(
            IReleaseRepository releaseRepo,
            IUserRepository userRepo,
            IEmailService emailService,
            ILogger<ReleaseNotificationService> logger)
        {
            _releaseRepo = releaseRepo;
            _userRepo = userRepo;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ReleaseNotifyResult> NotifyManagersAndAdminsAsync(
            int releaseId,
            string notificationType,
            string subject,
            string bodyHtml)
        {
            var result = new ReleaseNotifyResult();

            try
            {
                var users = await _userRepo.GetAllUsersAsync();
                var managers = users.Where(u =>
                    u.Active &&
                    !string.IsNullOrWhiteSpace(u.Email) &&
                    !string.IsNullOrWhiteSpace(u.RoleName) &&
                    (u.RoleName.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                     u.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase)));

                foreach (var u in managers)
                {
                    result.Recipients++;
                    var notificationId = await _releaseRepo.AddNotificationAsync(
                        releaseId, notificationType, u.UserId, u.Email,
                        $"Notify {u.UserName}: {subject}");

                    try
                    {
                        await _emailService.SendAsync(u.Email, subject, bodyHtml);
                        await _releaseRepo.MarkNotificationAsync(notificationId, "Sent");
                        result.Sent++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to email release notification to {Email}", u.Email);
                        await _releaseRepo.MarkNotificationAsync(notificationId, "Failed");
                        result.Failed++;
                    }
                }
            }
            catch (Exception ex)
            {
                // Notification failure must never fail the caller's primary action
                // (activation, or the background readiness scan).
                _logger.LogError(ex, "Failed to dispatch '{NotificationType}' notifications for Release {ReleaseId}", notificationType, releaseId);
            }

            return result;
        }
    }
}
