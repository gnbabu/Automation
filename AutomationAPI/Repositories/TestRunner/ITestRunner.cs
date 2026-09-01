namespace AutomationAPI.Repositories.TestRunner
{
    public interface ITestRunner
    {
        Task<List<TestExecutionResult>> RunAsync(string libsPath, string? library, string? className, string? methodName);
    }
}
