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
                var queueRepo = scope.ServiceProvider.GetRequiredService<ITestCaseExecutionQueueRepository>();
                var resultsRepo = scope.ServiceProvider.GetRequiredService<ITestCaseAssignmentRepository>();

                var pendingItems = await queueRepo.GetPendingExecutionQueuesAsync();

                foreach (var queue in pendingItems)
                {
                    try
                    {
                        var runner = scope.ServiceProvider.GetRequiredService<ITestRunner>();

                        var results = await runner.RunAsync(queue.LibraryName, queue.ClassName, queue.MethodName);

                        foreach (var result in results)
                        {
                            double durationSeconds = 0;

                            if (result.StartTime.HasValue && result.EndTime.HasValue)
                            {
                                TimeSpan testDuration = result.EndTime.Value - result.StartTime.Value;
                                durationSeconds = testDuration.TotalSeconds;   // <-- double
                            }

                            var tesrResult = new AssignedTestCaseStatusUpdate
                            {
                                AssignmentTestCaseId = queue.AssignmentTestCaseId,
                                TestCaseStatus = result.Passed ? "Passed" : "Failed",
                                Duration = durationSeconds,     
                                StartTime = result.StartTime,
                                EndTime = result.EndTime,
                                ErrorMessage = result.Message
                            };

                            await resultsRepo.UpdateAssignedTestCaseStatusAsync(tesrResult);

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
