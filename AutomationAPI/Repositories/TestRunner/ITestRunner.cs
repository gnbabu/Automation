namespace AutomationAPI.Repositories.TestRunner
{
    public interface ITestRunner
    {
        Task<List<TestExecutionResult>> RunAsync(string? library, string? className, string? methodName);
    }
}
