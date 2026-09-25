using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.TestRunner
{
    // Mirrors TestQueueWorker's own structure (IServiceProvider injection, CreateScope()
    // per cycle, Task.Delay loop). Does not execute anything itself - each cycle it calls
    // the *existing* ITestCaseExecutionQueueRepository.BulkScheduleAsync (the same method
    // Bulk Schedule already uses) to insert fresh aut.TestCaseExecutionQueue rows, which
    // TestQueueWorker's own 10s poll then picks up and runs exactly like any manually
    // scheduled item - execution itself is completely unchanged.
    public class RecurringScheduleWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(60);

        public RecurringScheduleWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var scheduleRepo = scope.ServiceProvider.GetRequiredService<IRecurringScheduleRepository>();
                var queueRepo = scope.ServiceProvider.GetRequiredService<ITestCaseExecutionQueueRepository>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<RecurringScheduleWorker>>();

                try
                {
                    var dueSchedules = await scheduleRepo.GetDueAsync(DateTime.Now);

                    foreach (var schedule in dueSchedules)
                    {
                        try
                        {
                            await ProcessScheduleAsync(scope.ServiceProvider, scheduleRepo, queueRepo, schedule, logger);
                        }
                        catch (Exception ex)
                        {
                            // Don't advance NextRunDate on a per-schedule failure - it'll
                            // simply be picked up again (and retried) on the next 60s cycle,
                            // same defensive intent as TestQueueWorker's own per-item catch.
                            logger.LogError(ex, "Failed to process recurring schedule {RecurringScheduleId}", schedule.RecurringScheduleId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in RecurringScheduleWorker poll cycle");
                }

                await Task.Delay(PollInterval, stoppingToken);
            }
        }

        private static async Task ProcessScheduleAsync(
            IServiceProvider services,
            IRecurringScheduleRepository scheduleRepo,
            ITestCaseExecutionQueueRepository queueRepo,
            RecurringScheduleDueModel schedule,
            ILogger logger)
        {
            // A Completed/Rejected release is permanently done, unlike the transient
            // "release folder not resolvable yet" case TestQueueWorker retries forever -
            // auto-pause here instead, with a visible reason, rather than silently
            // retrying every cycle with nothing ever actually firing.
            if (!string.IsNullOrWhiteSpace(schedule.ReleaseLifecycle) &&
                !schedule.ReleaseLifecycle.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                var reason = $"Release '{schedule.ReleaseName}' is {schedule.ReleaseLifecycle}.";
                await scheduleRepo.MarkRunAsync(schedule.RecurringScheduleId, null, isActive: false, pausedReason: reason);
                await NotifyPausedAsync(services, schedule, logger);
                return;
            }

            // Dynamic scope: looked up fresh every cycle, so test cases added to the
            // assignment after the schedule was created are automatically included.
            // Deliberately does NOT exclude 'Passed' (unlike the one-time Schedule
            // dialog's isTestCaseSelectable) - re-running already-passed tests on a
            // cadence is the entire point of a regression schedule. Only test cases
            // still in-flight (Queued/Scheduled/InProgress) are skipped, to avoid
            // double-queuing something a human or another schedule already queued.
            var eligibleIds = await scheduleRepo.GetEligibleTestCaseIdsAsync(schedule.AssignmentId);

            if (eligibleIds.Count > 0)
            {
                await queueRepo.BulkScheduleAsync(schedule.AssignmentId, eligibleIds, DateTime.Now, schedule.Browser, schedule.LoginUserId);
            }
            // Zero eligible test cases (all removed, or all currently in-flight) is a
            // no-op this cycle, not an error - NextRunDate still advances below.

            var nextRunDate = RecurrenceCalculator.ComputeNextRunDate(
                schedule.RecurrenceType, schedule.DaysOfWeek, schedule.DayOfMonth, schedule.TimeOfDay, DateTime.Now);

            var endDatePassed = schedule.EndDate.HasValue && schedule.EndDate.Value <= DateTime.Now;
            await scheduleRepo.MarkRunAsync(schedule.RecurringScheduleId, nextRunDate, isActive: !endDatePassed,
                pausedReason: endDatePassed ? "End date reached." : null);
        }

        // Reuses the same Admin/Manager recipient resolution as ReleaseNotificationService,
        // and the same email-try/in-app-try defensive split as the Notification Center -
        // a failure here must never affect the schedule's own MarkRunAsync state above.
        private static async Task NotifyPausedAsync(IServiceProvider services, RecurringScheduleDueModel schedule, ILogger logger)
        {
            try
            {
                var userRepo = services.GetRequiredService<IUserRepository>();
                var emailService = services.GetRequiredService<IEmailService>();
                var notificationRepo = services.GetRequiredService<INotificationRepository>();
                var configuration = services.GetRequiredService<IConfiguration>();

                var users = await userRepo.GetAllUsersAsync();
                var recipients = users.Where(u =>
                    u.Active &&
                    !string.IsNullOrWhiteSpace(u.Email) &&
                    !string.IsNullOrWhiteSpace(u.RoleName) &&
                    (u.RoleName.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                     u.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase)));

                var frontendUrl = configuration["App:FrontendUrl"]?.TrimEnd('/') ?? "";
                var subject = $"Recurring schedule paused: {schedule.AssignmentName}";
                var body = EmailTemplateBuilder.BuildRecurringSchedulePausedEmail(
                    schedule.AssignmentName, schedule.ReleaseName, schedule.ReleaseLifecycle, $"{frontendUrl}/recurring-schedules");

                foreach (var u in recipients)
                {
                    try
                    {
                        await emailService.SendAsync(u.Email, subject, body);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to email recurring-schedule-paused notification to {Email}", u.Email);
                    }

                    if (u.UserId.HasValue)
                    {
                        try
                        {
                            await notificationRepo.AddAsync(u.UserId.Value, "RecurringSchedulePaused", subject,
                                $"Release '{schedule.ReleaseName}' is {schedule.ReleaseLifecycle}.", "/recurring-schedules",
                                "RecurringSchedule", schedule.RecurringScheduleId);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to write in-app notification for {UserId}", u.UserId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Notification failure must never prevent the schedule from being marked
                // paused - same defensive reasoning already used everywhere notifications
                // are dispatched in this codebase.
                logger.LogError(ex, "Failed to dispatch recurring-schedule-paused notifications for RecurringScheduleId {RecurringScheduleId}", schedule.RecurringScheduleId);
            }
        }
    }
}
