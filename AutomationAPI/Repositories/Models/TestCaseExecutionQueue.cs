namespace AutomationAPI.Repositories.Models
{
    public class TestCaseExecutionQueue
    {
        public int Id { get; set; }
        public Guid QueueId { get; set; }
        public int AssignmentId { get; set; }
        public int AssignmentTestCaseId { get; set; }

        public string QueueStatus { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? ExecutionDateTime { get; set; }
    }

    public class SingleRunNowRequest
    {
        public int AssignmentId { get; set; }
        public int AssignmentTestCaseId { get; set; }
        public string? Browser { get; set; }
    }

    public class BulkRunNowRequest
    {
        public int AssignmentId { get; set; }
        public List<int> AssignmentTestCaseIds { get; set; } = new();
        public string? Browser { get; set; }
    }

    public class SingleScheduleRequest
    {
        public int AssignmentId { get; set; }
        public int AssignmentTestCaseId { get; set; }

        public DateTime ScheduleDate { get; set; }
        public string Browser { get; set; }
    }
    public class BulkScheduleRequest
    {
        public int AssignmentId { get; set; }
        public List<int> AssignmentTestCaseIds { get; set; } = new();
        public DateTime ScheduleDate { get; set; }
        public string Browser { get; set; }
    }

    public class QueueCreateResponse
    {
        public int Id { get; set; }
        public Guid QueueId { get; set; }
    }

    public enum QueueStatus
    {
        Queued = 1,          // Added to queue, waiting to be picked
        Scheduled = 2,       // Will run at a later date/time
        InProgress = 3,      // Execution started
        Completed = 4,       // Execution finished successfully
        Failed = 5,          // Execution finished with failure
        Cancelled = 6        // Cancelled by user before execution
    }
    public enum TestCaseStatus
    {
        Assigned = 1,        // Default - available for execution
        Queued = 2,          // Added to queue, cannot run again
        Scheduled = 3,       // Scheduled for future run
        Executing = 4,       // Currently running
        Completed = 5,       // Finished successfully
        Failed = 6,          // Finished with errors
        Blocked = 7          // Blocked due to environment/user/config issue
    }




}
