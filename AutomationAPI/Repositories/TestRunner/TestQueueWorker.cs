using System.Threading.Tasks;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using NUnit.Framework.Interfaces;

namespace AutomationAPI.Repositories.TestRunner
{
    public class TestQueueWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TestQueueWorker> _logger;

        public TestQueueWorker(IServiceProvider serviceProvider, ILogger<TestQueueWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var queueRepo = scope.ServiceProvider.GetRequiredService<ITestCaseExecutionQueueRepository>();
                var resultsRepo = scope.ServiceProvider.GetRequiredService<ITestCaseAssignmentRepository>();
                var releaseRepo = scope.ServiceProvider.GetRequiredService<IReleaseRepository>();
                var testFailureNotifier = scope.ServiceProvider.GetRequiredService<ITestExecutionNotificationService>();

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

                    // Captured before QueueStatus gets overwritten to "InProgress" below -
                    // this is the only place the original Queued-vs-Scheduled distinction
                    // is still available in-memory. Used only to decide whether a failure
                    // notification should fire - Run Now/Bulk Run Now
                    // failures are already visible immediately to whoever triggered them.
                    bool wasScheduled = queue.QueueStatus == "Scheduled";

                    try
                    {
                        var runner = scope.ServiceProvider.GetRequiredService<ITestRunner>();
                        var tokenGenerator = scope.ServiceProvider.GetRequiredService<ServiceTokenGenerator>();

                        // Marked here, server-side, right before the run actually starts -
                        // not via a push from inside the isolated test process (which was
                        // considered and rejected: it would only fire if the isolated
                        // process successfully starts and reaches OneTimeSetUp, leaving a
                        // launch failure stuck showing "Queued" forever with no signal an
                        // attempt was made; this way it's unconditional). Gives the Portal
                        // a mid-flight signal distinct from "queued, not started yet" for
                        // tests that can run 12-30+ seconds.
                        queue.QueueStatus = "InProgress";
                        await queueRepo.UpdateQueueStatusAsync(queue.QueueId, queue.QueueStatus);

                        var results = await runner.RunAsync(new TestRunRequest
                        {
                            LibsPath = releaseFolderPath,
                            Library = queue.LibraryName,
                            ClassName = queue.ClassName,
                            MethodName = queue.MethodName,
                            Browser = queue.Browser,
                            AccessToken = tokenGenerator.GenerateTestRunnerToken(),
                            AssignmentId = queue.AssignmentId,
                            AssignmentTestCaseId = queue.AssignmentTestCaseId,
                            LoginUserId = queue.LoginUserId,
                            EnvironmentId = queue.EnvironmentId
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
                                Console.WriteLine($"Warning: queue item {queue.QueueId} ran in-process (not isolated) - the Release folder is likely missing a full publish output.");
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

                            if (wasScheduled && tesrResult.TestCaseStatus == "Failed")
                            {
                                await testFailureNotifier.NotifyScheduledFailureAsync(
                                    queue.AssignmentTestCaseId,
                                    queue.TestCaseId ?? queue.AssignmentTestCaseId.ToString(),
                                    queue.Environment,
                                    tesrResult.ErrorMessage,
                                    queue.AssignedUser);
                            }
                        }

                        queue.QueueStatus = "Completed";
                        //queue.CompletedAt = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        queue.QueueStatus = "Failed";
                        //queue.CompletedAt = DateTime.UtcNow;

                        _logger.LogError(ex,
                            "Queue item {QueueId} (AssignmentTestCaseId {AssignmentTestCaseId}) failed before " +
                            "producing a test result - the test case's own status would previously have been " +
                            "left stuck on 'Queued' forever with no record of what happened.",
                            queue.QueueId, queue.AssignmentTestCaseId);

                        // Previously only the queue item itself was marked Failed here - the
                        // test case's own status (what the Portal grid actually shows) was
                        // never touched, so a launch-level failure (bad DLL, missing
                        // dependency the fallback couldn't resolve, etc. - anything that
                        // throws before RunAsync returns a normal result list) left it stuck
                        // showing "Queued" indefinitely instead of "Failed".
                        try
                        {
                            await resultsRepo.UpdateAssignedTestCaseStatusAsync(new AssignedTestCaseStatusUpdate
                            {
                                AssignmentTestCaseId = queue.AssignmentTestCaseId,
                                TestCaseStatus = "Failed",
                                ErrorMessage = ex.Message
                            });
                        }
                        catch (Exception statusEx)
                        {
                            _logger.LogError(statusEx,
                                "Failed to update AssignmentTestCaseId {AssignmentTestCaseId} to Failed after queue item {QueueId} itself failed.",
                                queue.AssignmentTestCaseId, queue.QueueId);
                        }
                    }

                    await queueRepo.UpdateQueueStatusAsync(queue.QueueId, queue.QueueStatus);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
