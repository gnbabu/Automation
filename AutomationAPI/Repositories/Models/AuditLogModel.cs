namespace AutomationAPI.Repositories.Models
{
    // A single audit trail entry - see AuditLog_Migration.sql for the full rationale.
    // EntityId/ActorUserId are nullable (Flow-level actions have no single numeric id;
    // background/system actions have no logged-in user); EntityName/ActorUserName are
    // always populated snapshots so the log stays readable even after the underlying
    // row/user is gone.
    public class AuditLogModel
    {
        public int AuditLogId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int? ActorUserId { get; set; }
        public string ActorUserName { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class AuditLogPagedResult
    {
        public IEnumerable<AuditLogModel> Items { get; set; } = Array.Empty<AuditLogModel>();
        public int TotalCount { get; set; }
    }

    public class AuditLogFilter
    {
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public int? ActorUserId { get; set; }
        public string? Action { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    // A single changed field, used to build a human-readable Details diff for Update
    // actions - e.g. {"changes":[{"field":"SectionName","old":"Foo","new":"Bar"}]}.
    public class AuditFieldChange
    {
        public string Field { get; set; } = string.Empty;
        public object? Old { get; set; }
        public object? New { get; set; }
    }
}
