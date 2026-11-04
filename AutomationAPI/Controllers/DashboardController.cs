using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        public const int MinTrendDays = 7;
        public const int MaxTrendDays = 30;

        private readonly ITestCaseExecutionQueueRepository _queueRepository;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ITestCaseExecutionQueueRepository queueRepository, ILogger<DashboardController> logger)
        {
            _queueRepository = queueRepository;
            _logger = logger;
        }

        public static int ClampTrendDays(int days) => Math.Clamp(days, MinTrendDays, MaxTrendDays);

        public static int? TrendScopeUser(bool isTester, int? userId) => isTester ? userId : null;

        [HttpGet("execution-trend")]
        public async Task<IActionResult> GetExecutionTrend([FromQuery] int releaseId, [FromQuery] int days = MinTrendDays)
        {
            if (releaseId <= 0) return BadRequest("ReleaseId is required.");

            var userId = this.GetAuditUserId();
            var isTester = User.IsInRole("Tester");
            if (isTester && userId == null) return Unauthorized();

            try
            {
                var points = await _queueRepository.GetReleaseExecutionTrendAsync(releaseId, ClampTrendDays(days), TrendScopeUser(isTester, userId));
                return Ok(points);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load the execution trend for release {ReleaseId}", releaseId);
                return StatusCode(500, "Failed to load the execution trend.");
            }
        }
    }
}
