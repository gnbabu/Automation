using AutomationAPI.Repositories.Interfaces;

namespace AutomationAPI.Repositories.TestRunner
{
    // Runs once at startup, in the background, to pre-populate NUnitEngineHelper's static
    // Explore() cache for every existing Release's folder - without this, the first real
    // request to GET /api/Release (used by both Release Management and the Dashboard,
    // via PopulateFolderInfoAsync -> GetTotalTestCaseCountAsync) pays the full reflection
    // discovery cost for every DLL of every release, since that cache starts empty on
    // every API restart. This eliminates that "slow the first time after logging in"
    // wait for real users by paying the cost once here instead, concurrently with the
    // app starting to accept requests.
    public class ExploreCacheWarmupWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExploreCacheWarmupWorker> _logger;

        public ExploreCacheWarmupWorker(IServiceProvider serviceProvider, ILogger<ExploreCacheWarmupWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var releaseRepo = scope.ServiceProvider.GetRequiredService<IReleaseRepository>();
            var testSuitesRepo = scope.ServiceProvider.GetRequiredService<ITestSuitesRepository>();

            try
            {
                var releases = await releaseRepo.GetAllAsync();
                foreach (var release in releases)
                {
                    if (stoppingToken.IsCancellationRequested) return;
                    if (string.IsNullOrWhiteSpace(release.ReleaseFolderPath)) continue;

                    try
                    {
                        await testSuitesRepo.GetTotalTestCaseCountAsync(release.ReleaseFolderPath);
                    }
                    catch (Exception ex)
                    {
                        // One release's folder being unreachable/missing shouldn't stop the
                        // rest from warming up - the real endpoint already tolerates this
                        // per-release too.
                        _logger.LogWarning(ex, "Failed to warm up test discovery cache for Release {ReleaseId}", release.ReleaseId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Explore cache warmup failed - first real page load will pay the discovery cost instead");
            }
        }
    }
}
