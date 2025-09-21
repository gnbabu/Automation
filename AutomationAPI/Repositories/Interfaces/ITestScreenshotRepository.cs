using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface ITestScreenshotRepository
    {
        Task<int> InsertScreenshotAsync(TestScreenshot screenshot);
        Task BulkInsertScreenshotsAsync(IEnumerable<TestScreenshot> screenshots);
        Task<IEnumerable<TestScreenshot>> GetScreenshotsByQueueIdAsync(string queueId, string methodName);
        Task<TestScreenshot?> GetScreenshotByIdAsync(int id);
    }
}
