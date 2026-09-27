using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface ITestSuitesRepository
    {
        Task<IEnumerable<LibraryInfo>> GetLibrariesAsync(string releaseFolderPath);
        Task<IEnumerable<TestCaseModel>> GetAllTestCasesByLibrary(string releaseFolderPath, string libraryName);
        Task<int> GetTotalTestCaseCountAsync(string releaseFolderPath);
    }
}
