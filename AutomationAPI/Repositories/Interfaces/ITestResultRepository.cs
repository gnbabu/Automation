using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface ITestResultRepository
    {
        Task<PagedResult<TestResult>> SearchTestResultsAsync(TestResultPayload payload);
        Task<IEnumerable<TestResult>> GetAllTestResultsAsync();

        Task<IEnumerable<TestResult>> GetTestResultsByQueueIdAsync(string queueId);
        Task<int> InsertTestResultsAsync(TestResult testResult);

    }
}
