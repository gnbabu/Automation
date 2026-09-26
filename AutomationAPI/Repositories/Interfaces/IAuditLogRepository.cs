using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<int> InsertAsync(
            string entityType, int? entityId, string entityName, string action,
            int? actorUserId, string actorUserName, string? details);

        Task<AuditLogPagedResult> GetPagedAsync(AuditLogFilter filter);

        Task<IEnumerable<string>> GetDistinctEntityTypesAsync();
    }
}
