using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IAuditLogService
    {
        // Fire-and-forget in spirit (awaited, but failures are caught and logged rather
        // than propagated) - an audit-log write must never fail or roll back the real
        // mutation it's attached to.
        Task LogAsync(
            string entityType, int? entityId, string entityName, string action,
            int? actorUserId, string actorUserName, IEnumerable<AuditFieldChange>? changes = null,
            object? snapshot = null);

        Task<AuditLogPagedResult> GetPagedAsync(AuditLogFilter filter);

        Task<IEnumerable<string>> GetDistinctEntityTypesAsync();
    }
}
