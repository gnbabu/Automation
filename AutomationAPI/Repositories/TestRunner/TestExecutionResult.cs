namespace AutomationAPI.Repositories.TestRunner
{
    // Mirrors NUnit's own result outcomes (a test-case's `result` attribute in the engine's
    // XML) instead of the old binary Passed/Failed - so an [Ignore]d/[Explicit] test can be
    // reported honestly as Skipped instead of being force-mapped into Pass or Fail.
    public enum TestOutcome
    {
        Passed,
        Failed,
        Skipped,
        Inconclusive
    }

    public class TestExecutionResult
    {
        public string? QueueId { get; set; }
        public string Name { get; set; } = "";
        public string ClassName { get; set; } = "";
        public TestOutcome Outcome { get; set; } = TestOutcome.Inconclusive;

        // Kept for backward compatibility with existing callers (e.g. TestQueueWorker's
        // current "Passed ? "Passed" : "Failed"" mapping) - true only for Outcome.Passed.
        public bool Passed => Outcome == TestOutcome.Passed;
        public string Message { get; set; } = "";
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        // True when this result came from an isolated child process (NUnit.Engine
        // ProcessModel=Separate). False when the runner had to fall back to running
        // in-process - e.g. because the Release folder only has the bare test DLL without
        // a full publish output (.deps.json/NUnit.Framework.dll/etc.) alongside it, which
        // an isolated child process needs and cannot resolve any other way. Surfaced so a
        // "silently reduced reliability" state is visible/discoverable instead of hidden.
        public bool WasIsolated { get; set; }
    }
}
