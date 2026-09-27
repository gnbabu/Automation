namespace AutomationAPI.Repositories.Interfaces
{
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
