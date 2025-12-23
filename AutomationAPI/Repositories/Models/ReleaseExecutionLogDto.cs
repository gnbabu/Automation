namespace AutomationAPI.Repositories.Models
{
    public class ReleaseExecutionLogDto
    {
        public string ReleaseName { get; set; }
        public int AssignmentId { get; set; }
        public int AssignmentTestCaseId { get; set; }
        public string TestCaseId { get; set; }
        public string TestCaseDescription { get; set; }

        public string StepName { get; set; }
        public string LogMessage { get; set; }
        public TestCaseLogLevel LogLevel { get; set; }
        public ExecutionStatus ExecutionStatus { get; set; }

        public int? ScreenshotId { get; set; }
        public string? ErrorStackTrace { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
