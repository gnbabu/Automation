namespace AutomationAPI.Repositories.Interfaces
{
    /// <summary>
    /// Sends a failure notification for a Scheduled test run (only - Run Now/Bulk Run
    /// Now failures are already visible immediately to whoever triggered them, watching
    /// the same screen) to the assigned user + all active Admins, recording each attempt
    /// in aut.TestExecutionNotification. No automatic retry - this is purely informational
    /// so a human decides whether to retry (see AGENTS.md); the actual retry is just a
    /// normal Run Now/Schedule click, now unlocked for non-passing statuses.
    /// </summary>
    public interface ITestExecutionNotificationService
    {
        Task NotifyScheduledFailureAsync(
            int assignmentTestCaseId,
            string testCaseId,
            string? environmentName,
            string? errorMessage,
            int? assignedUserId);
    }
}
