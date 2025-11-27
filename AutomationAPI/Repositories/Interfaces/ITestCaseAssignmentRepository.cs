using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface ITestCaseAssignmentRepository
    {
        Task<IEnumerable<TestCaseAssignment>> GetAllAssignmentsAsync();
        Task<IEnumerable<TestCaseAssignment>> GetAssignmentsByUserIdAsync(int userId);

        //New Assignment Implementation
        Task<IEnumerable<AssignedTestCase>> GetTestCasesByAssignmentNameAndUserAsync(string assignmentName, int assignedUserId);
        Task<IEnumerable<AssignedTestCase>> GetAllAssignedTestCasesInLibraryAsync(string libraryName);
        Task<IEnumerable<AssignedTestCase>> GetAssignedTestCasesForLibraryAndEnvironmentAsync(string libraryName, string environment);
        Task CreateOrUpdateAssignmentWithTestCasesAsync(AssignmentCreateUpdateRequest request);
    }
}
