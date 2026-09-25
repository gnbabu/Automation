using AutomationAPI.Repositories;
using AutomationAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class RecurringScheduleController : ControllerBase
    {
        private readonly IRecurringScheduleRepository _repo;
        private readonly ITestCaseAssignmentRepository _assignmentRepo;
        private readonly ILogger<RecurringScheduleController> _logger;

        public RecurringScheduleController(
            IRecurringScheduleRepository repo,
            ITestCaseAssignmentRepository assignmentRepo,
            ILogger<RecurringScheduleController> logger)
        {
            _repo = repo;
            _assignmentRepo = assignmentRepo;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var schedules = await _repo.GetAllAsync();
            return Ok(schedules);
        }

        [HttpGet("assignments")]
        public async Task<IActionResult> GetAssignmentOptions()
        {
            var options = await _repo.GetAssignmentOptionsAsync();
            return Ok(options);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RecurringScheduleRequest request)
        {
            try
            {
                if (request == null || request.AssignmentId <= 0)
                    return BadRequest("AssignmentId is required.");

                if (string.IsNullOrWhiteSpace(request.RecurrenceType) ||
                    !new[] { "Daily", "Weekly", "Monthly" }.Contains(request.RecurrenceType))
                    return BadRequest("RecurrenceType must be 'Daily', 'Weekly', or 'Monthly'.");

                if (request.RecurrenceType == "Weekly" && string.IsNullOrWhiteSpace(request.DaysOfWeek))
                    return BadRequest("DaysOfWeek is required for a Weekly schedule.");

                if (request.RecurrenceType == "Monthly" && (request.DayOfMonth is null or < 1 or > 31))
                    return BadRequest("DayOfMonth (1-31) is required for a Monthly schedule.");

                if (string.IsNullOrWhiteSpace(request.Browser))
                    return BadRequest("Browser is required.");

                // Same lifecycle guard Run Now/Schedule already enforce - reused here so a
                // recurring schedule can't be created against a release that's already
                // Completed/Rejected in the first place.
                var lifecycle = await _assignmentRepo.GetReleaseLifecycleForAssignmentAsync(request.AssignmentId);
                if (!string.IsNullOrWhiteSpace(lifecycle) && !lifecycle.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    return BadRequest($"This release is {lifecycle} and no longer accepts new test executions.");

                var nextRunDate = RecurrenceCalculator.ComputeNextRunDate(
                    request.RecurrenceType, request.DaysOfWeek, request.DayOfMonth, request.TimeOfDay, DateTime.Now);

                var id = await _repo.CreateAsync(
                    request.AssignmentId, request.RecurrenceType, request.DaysOfWeek, request.DayOfMonth,
                    request.TimeOfDay, request.Browser, request.LoginUserId, request.EndDate, nextRunDate,
                    User.Identity?.Name ?? "system");

                return Ok(new { RecurringScheduleId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recurring schedule for AssignmentId {AssignmentId}", request?.AssignmentId);
                return StatusCode(500, "An unexpected error occurred while creating the recurring schedule.");
            }
        }

        [HttpPost("{id:int}/pause")]
        public async Task<IActionResult> Pause(int id)
        {
            await _repo.SetActiveAsync(id, false);
            return Ok();
        }

        [HttpPost("{id:int}/resume")]
        public async Task<IActionResult> Resume(int id)
        {
            await _repo.SetActiveAsync(id, true);
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            return Ok();
        }
    }

    public class RecurringScheduleRequest
    {
        public int AssignmentId { get; set; }
        public string RecurrenceType { get; set; }
        // Nullable - only present for Weekly (Daily/Monthly omit it entirely). Without the
        // '?', [ApiController]'s automatic non-nullable-reference-type validation (Nullable
        // enabled project-wide) rejects the request with "DaysOfWeek field is required"
        // whenever it's legitimately absent.
        public string? DaysOfWeek { get; set; }
        public int? DayOfMonth { get; set; }
        public TimeSpan TimeOfDay { get; set; }
        public string Browser { get; set; }
        public int? LoginUserId { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
