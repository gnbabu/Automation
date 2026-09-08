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

        // Short-lived JWT (see ServiceTokenGenerator) threaded into the isolated test
        // process's TestParameters so it can call back into AutomationAPI's
        // [Authorize]-protected endpoints (e.g. Selenium.BaseComponents.Utilities.
        // APIGatway.GetAutomationData).
        public string? AccessToken { get; set; }

        // Threaded through so a test's BaseFeatureFixture can populate its
        // AssignmentId/AssignmentTestCaseId automatically (used for step-level log/
        // screenshot uploads, e.g. TC.PriorAuthSearch's SaveTestCaseLog/
        // SaveMethodScreenShots calls) - see Phase 3 in AGENTS.md. Deliberately not
        // including QueueId here: the one thing it would have been used for (an
        // in-test "InProgress" push) is handled server-side in TestQueueWorker instead,
        // before RunAsync is even called - simpler and doesn't depend on the isolated
        // process successfully starting.
        public int AssignmentId { get; set; }
        public int AssignmentTestCaseId { get; set; }

        // Set only when the environment required authentication and a specific login
        // user was explicitly picked at Run Now/Schedule time (see AGENTS.md) - resolved
        // by BaseFeatureFixture via a service-JWT-only API call. Null when the
        // environment doesn't require authentication, or for runs outside the queue
        // pipeline (falls back to today's hard-coded UserCredentials behavior).
        public int? LoginUserId { get; set; }

        // The Release's environment - resolved by BaseFeatureFixture via
        // GET api/Environment/{id} to get EnvironmentUrl/RequiresAuthentication.
        public int? EnvironmentId { get; set; }
    }

    public interface ITestRunner
    {
        Task<List<TestExecutionResult>> RunAsync(TestRunRequest request);
    }
}
