using AutomationAPI.Repositories.Interfaces;

namespace AutomationAPI.Repositories.Workers
{
    /// <summary>
    /// Proactively notifies active Manager/Admin users once a Draft release's release
    /// folder has usable DLL content, so they don't have to keep checking the Release
    /// Details page manually. This worker NEVER activates a release itself — activation
    /// remains a deliberate human action; the worker only sends an informational
    /// notification, exactly once per release, reusing the existing
    /// IReleaseReadinessService (read-only reflection check) and
    /// IReleaseNotificationService (same Manager/Admin recipient logic used by
    /// ReleaseController.Activate).
    /// </summary>
    public class ReleaseDllsReadyNotificationWorker : BackgroundService
    {
        private const string NotificationType = "DllsReadyForActivation";
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReleaseDllsReadyNotificationWorker> _logger;

        public ReleaseDllsReadyNotificationWorker(
            IServiceProvider serviceProvider,
            ILogger<ReleaseDllsReadyNotificationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ScanAsync();
                }
                catch (Exception ex)
                {
                    // Never let a scan failure kill the worker loop.
                    _logger.LogError(ex, "ReleaseDllsReadyNotificationWorker scan failed");
                }

                await Task.Delay(PollInterval, stoppingToken);
            }
        }

        private async Task ScanAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var releaseRepo = scope.ServiceProvider.GetRequiredService<IReleaseRepository>();
            var readinessService = scope.ServiceProvider.GetRequiredService<IReleaseReadinessService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<IReleaseNotificationService>();

            var releases = await releaseRepo.GetAllAsync();

            var draftReleases = releases.Where(r =>
                (r.ReleaseLifecycle ?? string.Empty).Equals("Draft", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(r.ReleaseFolderPath));

            foreach (var release in draftReleases)
            {
                DllsReadyForActivationOutcome outcome;
                try
                {
                    outcome = await EvaluateReleaseAsync(release, readinessService, releaseRepo, notificationService);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to evaluate DLL readiness for Release {ReleaseId}", release.ReleaseId);
                    continue;
                }

                if (outcome == DllsReadyForActivationOutcome.Notified)
                {
                    _logger.LogInformation("Sent DLLs-ready notification for Release {ReleaseId} ({ReleaseName})", release.ReleaseId, release.ReleaseName);
                }
            }
        }

        private enum DllsReadyForActivationOutcome
        {
            NotReady,
            AlreadyNotified,
            Notified
        }

        private async Task<DllsReadyForActivationOutcome> EvaluateReleaseAsync(
            Models.ReleaseModel release,
            IReleaseReadinessService readinessService,
            IReleaseRepository releaseRepo,
            IReleaseNotificationService notificationService)
        {
            var readiness = readinessService.CheckReadiness(release.ReleaseFolderPath);
            if (!readiness.IsReady)
                return DllsReadyForActivationOutcome.NotReady;

            var existingNotifications = await releaseRepo.GetNotificationsAsync(release.ReleaseId);
            var alreadyNotified = existingNotifications.Any(n =>
                (n.NotificationType ?? string.Empty).Equals(NotificationType, StringComparison.OrdinalIgnoreCase));

            if (alreadyNotified)
                return DllsReadyForActivationOutcome.AlreadyNotified;

            var subject = $"Release ready to activate: {release.ReleaseName} {release.Version}";
            var body = $"<p>Release <strong>{release.ReleaseName}</strong> (Version {release.Version}, " +
                       $"Environment {release.EnvironmentName}) now has usable DLLs in its release folder " +
                       $"and is ready for activation. Activation is a manual step — please review and " +
                       $"activate it from Release Management when ready.</p>";

            await notificationService.NotifyManagersAndAdminsAsync(release.ReleaseId, NotificationType, subject, body);
            return DllsReadyForActivationOutcome.Notified;
        }
    }
}
