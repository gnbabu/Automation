using System.Text.Json;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories
{
    // Thin wrapper around IAuditLogRepository - the only place that decides "what does
    // Details look like" (a changes[] diff for Updates, a plain snapshot object for
    // Creates/Deletes) and the only place that swallows-and-logs a write failure so it
    // can never affect the real mutation a caller is logging alongside.
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repo;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(IAuditLogRepository repo, ILogger<AuditLogService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task LogAsync(
            string entityType, int? entityId, string entityName, string action,
            int? actorUserId, string actorUserName, IEnumerable<AuditFieldChange>? changes = null,
            object? snapshot = null)
        {
            try
            {
                string? details = null;
                if (changes != null)
                {
                    var changeList = changes.ToList();
                    if (changeList.Count > 0)
                        details = JsonSerializer.Serialize(new { changes = changeList });
                }
                else if (snapshot != null)
                {
                    details = JsonSerializer.Serialize(snapshot);
                }

                await _repo.InsertAsync(
                    entityType, entityId, entityName, action,
                    actorUserId, string.IsNullOrWhiteSpace(actorUserName) ? "Unknown" : actorUserName,
                    details);
            }
            catch (Exception ex)
            {
                // Deliberately swallowed beyond logging - see IAuditLogService.LogAsync's
                // own comment. Losing one audit entry is far preferable to failing (or
                // worse, rolling back) the real Section/Environment/Release/etc. mutation
                // it was attached to.
                _logger.LogWarning(ex,
                    "Failed to write audit log entry for {EntityType}/{EntityId} action {Action}",
                    entityType, entityId, action);
            }
        }

        public Task<AuditLogPagedResult> GetPagedAsync(AuditLogFilter filter) => _repo.GetPagedAsync(filter);

        public Task<IEnumerable<string>> GetDistinctEntityTypesAsync() => _repo.GetDistinctEntityTypesAsync();
    }
}
