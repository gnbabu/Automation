namespace AutomationAPI.Repositories.TestRunner
{
    public class TestRunRequest
    {
        public string LibsPath { get; set; } = "";
        public string? Library { get; set; }
        public string? ClassName { get; set; }
        public string? MethodName { get; set; }
        public string? Browser { get; set; }
        public string? AccessToken { get; set; }
        public int AssignmentId { get; set; }
        public int AssignmentTestCaseId { get; set; }
        public int? LoginUserId { get; set; }
        public int? EnvironmentId { get; set; }
    }

    public interface ITestRunner
    {
        Task<List<TestExecutionResult>> RunAsync(TestRunRequest request);
    }
}
