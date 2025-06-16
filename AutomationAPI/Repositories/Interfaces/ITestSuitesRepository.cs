using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface ITestSuitesRepository
    {
        Task<IEnumerable<LibraryInfo>> GetLibrariesAsync();
    }
}
