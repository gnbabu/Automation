namespace Selenium.BaseComponents.Utilities
{
    public class TestResults
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ResultStatus { get; set; }
        public string Duration { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Message { get; set; }
        public string ClassName { get; set; }
        public string QueueId { get; set; }
    }

    public class Screeshots
    {
        public int AssignmentTestCaseId { get; set; }
        public string MethodName { get; set; }
        public List<Byte[]> screenShot { get; set; }
    }
    public class TestScreenshot
    {
        public int ID { get; set; }
        public int AssignmentTestCaseId { get; set; }
        public string Caption { get; set; } = string.Empty;
        public string Screenshot { get; set; } = string.Empty;
        public DateTime TakenAt { get; set; }
    }

    public class TestCaseExecutionLog
    {
        public int AssignmentId { get; set; }
        public int AssignmentTestCaseId { get; set; }
        public string TestCaseId { get; set; }
        public string TestCaseDescription { get; set; }
        public string StepName { get; set; }
        public string LogMessage { get; set; }
        public TestCaseLogLevel LogLevel { get; set; }
        public ExecutionStatus ExecutionStatus { get; set; }
        public int? ScreenshotId { get; set; }
        public string ErrorStackTrace { get; set; }
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
