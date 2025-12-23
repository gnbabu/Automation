using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface ITestCaseExecutionLogRepository
    {
        Task AddAsync(TestCaseExecutionLog log);
        Task<IEnumerable<TestCaseExecutionLog>> GetByTestCaseAsync(int assignmentId, int assignmentTestCaseId);
        Task<IEnumerable<TestCaseExecutionLog>> GetByAssignmentAsync(int assignmentId);
        Task<IEnumerable<ReleaseExecutionLogDto>> GetByReleaseAsync(string releaseName);
    }
}
