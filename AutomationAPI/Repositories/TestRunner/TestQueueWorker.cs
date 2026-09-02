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
                var releaseRepo = scope.ServiceProvider.GetRequiredService<IReleaseRepository>();

                var pendingItems = await queueRepo.GetPendingExecutionQueuesAsync();

                foreach (var queue in pendingItems)
                {
                    // Resolve the Release folder to execute from. If it can't be resolved
                    // (release deleted, not yet linked, or folder not set), skip this item
                    // for now and retry on the next cycle rather than failing it outright.
                    string releaseFolderPath = null;
                    if (queue.ReleaseId.HasValue)
                    {
                        var release = await releaseRepo.GetByIdAsync(queue.ReleaseId.Value);
                        releaseFolderPath = release?.ReleaseFolderPath;
                    }

                    if (string.IsNullOrWhiteSpace(releaseFolderPath))
                    {
                        Console.WriteLine($"Skipping queue item {queue.QueueId}: unable to resolve release folder for ReleaseId {queue.ReleaseId}. Will retry.");
                        continue;
                    }

                    try
                    {
                        var runner = scope.ServiceProvider.GetRequiredService<ITestRunner>();

                        var results = await runner.RunAsync(new TestRunRequest
                        {
                            LibsPath = releaseFolderPath,
                            Library = queue.LibraryName,
                            ClassName = queue.ClassName,
                            MethodName = queue.MethodName,
                            Browser = queue.Browser
                        });

                        foreach (var result in results)
                        {
                            double durationSeconds = 0;

                            if (result.StartTime.HasValue && result.EndTime.HasValue)
                            {
                                TimeSpan testDuration = result.EndTime.Value - result.StartTime.Value;
                                durationSeconds = testDuration.TotalSeconds;   // <-- double
                            }

                            if (!result.WasIsolated)
                            {
                                Console.WriteLine($"Warning: queue item {queue.QueueId} ran in-process (not isolated) - the Release folder is likely missing a full publish output. See AGENTS.md.");
                            }

                            var tesrResult = new AssignedTestCaseStatusUpdate
                            {
                                AssignmentTestCaseId = queue.AssignmentTestCaseId,
                                TestCaseStatus = result.Outcome switch
                                {
                                    TestOutcome.Passed => "Passed",
                                    TestOutcome.Skipped => "Skipped",
                                    TestOutcome.Inconclusive => "Skipped",
                                    _ => "Failed"
                                },
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
