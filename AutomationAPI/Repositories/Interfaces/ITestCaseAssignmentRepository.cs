using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface ITestCaseAssignmentRepository
    {
        
        //New Assignment Implementation
        Task<IEnumerable<TestCaseAssignmentEntity>> GetAssignmentsByUserIdAsync(int userId);
        Task<IEnumerable<AssignedTestCase>> GetTestCasesByAssignmentNameAndUserAsync(string assignmentName, int assignedUserId);
        Task<IEnumerable<AssignedTestCase>> GetAllAssignedTestCasesInLibraryAsync(string libraryName);
        Task<IEnumerable<AssignedTestCase>> GetAssignedTestCasesForLibraryAndEnvironmentAsync(string libraryName, string environment);
        Task CreateOrUpdateAssignmentWithTestCasesAsync(AssignmentCreateUpdateRequest request);

        Task<bool> UpdateAssignedTestCaseStatusAsync(AssignedTestCaseStatusUpdate request);
    }
}
