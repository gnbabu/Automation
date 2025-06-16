namespace AutomationAPI.Repositories.TestRunner
{
    public class TestExecutionResult
    {
        public string? QueueId { get; set; }
        public string Name { get; set; } = "";
        public string ClassName { get; set; } = "";
        public bool Passed { get; set; }
        public string Message { get; set; } = "";
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

}
