using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface ITestCaseAssignmentRepository
    {
        Task<IEnumerable<TestCaseAssignment>> GetAllAssignmentsAsync();
        Task<IEnumerable<TestCaseAssignment>> GetAssignmentsByUserIdAsync(int userId);
        Task BulkInsertAssignmentsOldAsync(IEnumerable<TestCaseAssignment> assignments);
        Task BulkInsertAssignmentsAsync(IEnumerable<TestCaseAssignment> assignments);
        Task DeleteAssignmentsByUserIdAsync(int userId);
        Task DeleteAssignmentsAsync(TestCaseAssignmentDeleteRequest request);
    }
}
