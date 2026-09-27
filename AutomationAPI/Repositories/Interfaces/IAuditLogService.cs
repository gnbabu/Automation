using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(
            string entityType, int? entityId, string entityName, string action,
            int? actorUserId, string actorUserName, IEnumerable<AuditFieldChange>? changes = null,
            object? snapshot = null);

        Task<AuditLogPagedResult> GetPagedAsync(AuditLogFilter filter);

        Task<IEnumerable<string>> GetDistinctEntityTypesAsync();
    }
}
