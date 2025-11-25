using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface ITestSuitesRepository
    {
        Task<IEnumerable<LibraryInfo>> GetLibrariesAsync();
        Task<IEnumerable<TestCaseModel>> GetAllTestCasesAsync(string? libraryName, bool? assigned);
        Task<IEnumerable<TestCaseModel>> GetAllTestCasesByLibrary(string libraryName);
    }
}
