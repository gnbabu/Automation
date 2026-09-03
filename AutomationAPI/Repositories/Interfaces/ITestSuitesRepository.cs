using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface ITestSuitesRepository
    {
        Task<IEnumerable<LibraryInfo>> GetLibrariesAsync(string releaseFolderPath);
        Task<IEnumerable<TestCaseModel>> GetAllTestCasesByLibrary(string releaseFolderPath, string libraryName);

        // Total test cases discoverable across every DLL in the Release's folder,
        // regardless of assignment - distinct from aut.TestCaseAssignment/AssignedTestCases
        // counts (which only ever reflect test cases someone has actually been assigned).
        // Cheap on repeat calls thanks to NUnitEngineHelper.Explore()'s last-write-time cache.
        Task<int> GetTotalTestCaseCountAsync(string releaseFolderPath);
    }
}
