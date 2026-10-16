using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface ITestDataRepository
    {
        Task<IEnumerable<TestDataTestCase>> GetMyTestCasesAsync(int userId, int environmentId);
        Task<TestDataAssignment?> ResolveAssignmentAsync(int assignmentTestCaseId);
        Task<IEnumerable<AutomationDataSection>> UpsertModelSectionsAsync(string flowName, IEnumerable<string> sectionNames);
        Task<TestDataDocumentRecord?> GetDocumentAsync(string libraryName, string className, string methodName, int userId, int environmentId);
        Task<TestDataSaveResult> SaveDocumentAsync(string libraryName, string className, string methodName,
            int userId, int environmentId, string flowName, string dataJson, long? expectedVersion);
        Task<IEnumerable<TestDataCopySource>> GetCopySourcesAsync(string libraryName, string className, string methodName,
            string flowName, int userId, int environmentId);
    }
}
