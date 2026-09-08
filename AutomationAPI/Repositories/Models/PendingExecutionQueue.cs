namespace AutomationAPI.Repositories.Models
{
    public class PendingExecutionQueue
    {
        public Guid QueueId { get; set; }
        public int AssignmentId { get; set; }
        public int AssignmentTestCaseId { get; set; }
        public int? ReleaseId { get; set; }
        public int? EnvironmentId { get; set; }
        public string? LibraryName { get; set; }
        public string? ClassName { get; set; }
        public string? MethodName { get; set; }
        public string? Environment { get; set; }
        public string? Browser { get; set; }

        // Set only when the environment required authentication and a specific login
        // user was explicitly picked at Run Now/Schedule time (see AGENTS.md). Null
        // otherwise - BaseFeatureFixture falls back to today's hard-coded credentials.
        public int? LoginUserId { get; set; }
        public string? QueueStatus { get; set; }
        public DateTime? ExecutionDateTime { get; set; }
    }
}
