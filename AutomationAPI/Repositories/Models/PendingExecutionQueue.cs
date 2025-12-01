namespace AutomationAPI.Repositories.Models
{
    public class PendingExecutionQueue
    {
        public Guid QueueId { get; set; }
        public int AssignmentTestCaseId { get; set; }
        public string? LibraryName { get; set; }
        public string? ClassName { get; set; }
        public string? MethodName { get; set; }
        public string? Environment { get; set; }
        public string? QueueStatus { get; set; }
        public DateTime? ExecutionDateTime { get; set; }
    }
}
