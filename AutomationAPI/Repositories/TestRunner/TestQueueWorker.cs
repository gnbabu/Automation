using System.Threading.Tasks;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using NUnit.Framework.Interfaces;

namespace AutomationAPI.Repositories.TestRunner
{
    public class TestQueueWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public TestQueueWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var queueRepo = scope.ServiceProvider.GetRequiredService<IQueueRepository>();
                var resultsRepo = scope.ServiceProvider.GetRequiredService<ITestResultRepository>();

                var pendingItems = await queueRepo.GetAllQueuesAsync();

                pendingItems = pendingItems.Where(q => q.QueueStatus == "New");

                foreach (var queue in pendingItems)
                {
                    try
                    {
                        var runner = scope.ServiceProvider.GetRequiredService<ITestRunner>();

                        var results = await runner.RunAsync(queue.LibraryName, queue.ClassName, queue.MethodName);

                        foreach (var result in results)
                        {
                            var duration = string.Empty;

                            if (result.StartTime.HasValue && result.EndTime.HasValue)
                            {
                                // Calculate the time span and format the duration
                                TimeSpan testDuration = result.EndTime.Value - result.StartTime.Value;
                                duration = testDuration.TotalSeconds.ToString("F2"); // Store duration in seconds with 2 decimals
                            }

                            var tesrResult = new TestResult
                            {
                                Name = result.Name,                                 // Set this to the name of the test
                                ResultStatus = result.Passed ? "Passed" : "Failed",  // "Passed" or "Failed"
                                Duration = duration,    // Duration as a string
                                StartTime = result.StartTime,  // DateTime for start
                                EndTime = result.EndTime,      // DateTime for end
                                Message = result.Message,      // Any relevant message (error, success, etc.)
                                ClassName = result.ClassName,  // Name of the test class
                                QueueId = queue.QueueId         // QueueId if applicable
                            };

                            await resultsRepo.InsertTestResultsAsync(tesrResult);
                        }

                        queue.QueueStatus = "Completed";
                        //queue.CompletedAt = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        queue.QueueStatus = "Failed";
                        //queue.CompletedAt = DateTime.UtcNow;
                        // Optionally log exception
                    }

                    await queueRepo.UpdateQueueStatusAsync(queue.QueueId, queue.QueueStatus);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
