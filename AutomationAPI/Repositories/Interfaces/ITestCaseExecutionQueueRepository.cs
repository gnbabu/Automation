using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface ITestCaseExecutionQueueRepository
    {
        Task<(int Id, Guid QueueId)> SingleRunNowAsync(int assignmentId, int assignmentTestCaseId, string browser);

        Task<bool> BulkRunNowAsync(int assignmentId, List<int> assignmentTestCaseIds, string browser);
        Task<(int Id, Guid QueueId)> SingleScheduleAsync(int assignmentId, int assignmentTestCaseId, DateTime scheduleDate, string browser);

        Task<bool> BulkScheduleAsync(int assignmentId, List<int> assignmentTestCaseIds, DateTime scheduleDate, string browser);
        Task<IEnumerable<PendingExecutionQueue>> GetPendingExecutionQueuesAsync();
        Task<int> UpdateQueueStatusAsync(Guid queueId, string status);

    }
}
