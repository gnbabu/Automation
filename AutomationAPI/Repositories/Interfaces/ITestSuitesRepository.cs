using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface ITestSuitesRepository
    {
        Task<IEnumerable<LibraryInfo>> GetLibrariesAsync(string releaseFolderPath);
        Task<IEnumerable<TestCaseModel>> GetAllTestCasesByLibrary(string releaseFolderPath, string libraryName);
    }
}
