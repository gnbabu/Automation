using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface ITestCaseAssignmentRepository
    {
        Task<IEnumerable<TestCaseAssignment>> GetAssignmentsByUserIdAsync(int userId);
        Task BulkInsertAssignmentsAsync(IEnumerable<TestCaseAssignment> assignments);
        Task DeleteAssignmentsByUserIdAsync(int userId);
    }
}
