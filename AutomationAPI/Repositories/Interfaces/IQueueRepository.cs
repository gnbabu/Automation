using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IQueueRepository
    {
        Task<IEnumerable<QueueInfo>> GetAllQueuesAsync();
        Task<QueueInfo> GetQueueByIdAsync(string queueId);
        Task<IEnumerable<QueueInfo>> GetQueuesAsync(string? queueId, string? queueStatus);
        Task<int> InsertQueueAsync(QueueInfo queue);
        Task<int> UpdateQueueStatusAsync(string queueId, string queueStatus);
        Task<bool> DeleteQueueAsync(string queueId);
        Task<PagedResult<QueueInfo>> GetQueueReportsAsync(QueueReportFilterRequest filter);
    }
}
