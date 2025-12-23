namespace AutomationAPI.Repositories.Models
{
    public class TestCaseExecutionLog
    {
        public long? LogId { get; set; }
        public int AssignmentId { get; set; }
        public int AssignmentTestCaseId { get; set; }

        public string TestCaseId { get; set; }
        public string TestCaseDescription { get; set; }

        public string StepName { get; set; }
        public string LogMessage { get; set; }
        public TestCaseLogLevel LogLevel { get; set; } // -- Info | Pass | Fail | Warning
        public ExecutionStatus ExecutionStatus { get; set; }// -- Running | Passed | Failed

        public int? ScreenshotId { get; set; }
        public string? ErrorStackTrace { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public enum TestCaseLogLevel
    {
        Info,
        Pass,
        Warning,
        Fail
    }

    public enum ExecutionStatus
    {
        Running,
        Passed,
        Failed
    }

}
