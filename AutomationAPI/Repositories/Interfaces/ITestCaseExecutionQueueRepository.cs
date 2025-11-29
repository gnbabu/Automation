namespace AutomationAPI.Repositories.Interfaces
{
    public interface ITestCaseExecutionQueueRepository
    {
        Task<(int Id, Guid QueueId)> SingleRunNowAsync(int assignmentId, int assignmentTestCaseId);

        Task<bool> BulkRunNowAsync(int assignmentId, List<int> assignmentTestCaseIds);
        Task<(int Id, Guid QueueId)> SingleScheduleAsync(int assignmentId, int assignmentTestCaseId, DateTime scheduleDate);

        Task<bool> BulkScheduleAsync(int assignmentId, List<int> assignmentTestCaseIds, DateTime scheduleDate);
    }
}
