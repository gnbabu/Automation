namespace AutomationAPI.Repositories.TestRunner
{
    // Bundled into one object (rather than growing the old 4-string-parameter list further)
    // now that Browser needs to flow through too - see TestQueueWorker/NUnitEngineTestRunner.
    public class TestRunRequest
    {
        public string LibsPath { get; set; } = "";
        public string? Library { get; set; }
        public string? ClassName { get; set; }
        public string? MethodName { get; set; }
        public string? Browser { get; set; }
    }

    public interface ITestRunner
    {
        Task<List<TestExecutionResult>> RunAsync(TestRunRequest request);
    }
}
