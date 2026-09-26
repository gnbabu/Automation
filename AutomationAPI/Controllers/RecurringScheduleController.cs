using AutomationAPI.Repositories;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Models;
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
        private readonly IAuditLogService _auditLog;
        private readonly ILogger<RecurringScheduleController> _logger;

        public RecurringScheduleController(
            IRecurringScheduleRepository repo,
            ITestCaseAssignmentRepository assignmentRepo,
            IAuditLogService auditLog,
            ILogger<RecurringScheduleController> logger)
        {
            _repo = repo;
            _assignmentRepo = assignmentRepo;
            _auditLog = auditLog;
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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var schedule = await _repo.GetByIdAsync(id);
            if (schedule == null)
                return NotFound();
            return Ok(schedule);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RecurringScheduleRequest request)
        {
            try
            {
                if (request == null || request.AssignmentId <= 0)
                    return BadRequest("AssignmentId is required.");

                var validationError = ValidateRecurrence(request);
                if (validationError != null)
                    return BadRequest(validationError);

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

                var created = await _repo.GetByIdAsync(id);
                await _auditLog.LogAsync("RecurringSchedule", id, created?.AssignmentName ?? $"Assignment #{request.AssignmentId}", "Created",
                    this.GetAuditUserId(), this.GetAuditUserName(),
                    snapshot: new { assignmentId = request.AssignmentId, recurrenceType = request.RecurrenceType, daysOfWeek = request.DaysOfWeek, dayOfMonth = request.DayOfMonth, timeOfDay = request.TimeOfDay, browser = request.Browser });

                return Ok(new { RecurringScheduleId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recurring schedule for AssignmentId {AssignmentId}", request?.AssignmentId);
                return StatusCode(500, "An unexpected error occurred while creating the recurring schedule.");
            }
        }

        // The Assignment itself is deliberately not editable here (see
        // Recurring_Schedule_Update_Migration.sql's own comment) - only cadence/execution
        // settings, so the release-lifecycle guard from Create doesn't need to be re-checked
        // (an already-paused/auto-paused schedule staying paused after an unrelated edit like
        // changing the time-of-day is fine; resuming it still goes through the worker's own
        // lifecycle check on its next due cycle).
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] RecurringScheduleRequest request)
        {
            try
            {
                var validationError = ValidateRecurrence(request);
                if (validationError != null)
                    return BadRequest(validationError);

                var existing = await _repo.GetByIdAsync(id);
                if (existing == null)
                    return NotFound();

                var nextRunDate = RecurrenceCalculator.ComputeNextRunDate(
                    request.RecurrenceType, request.DaysOfWeek, request.DayOfMonth, request.TimeOfDay, DateTime.Now);

                await _repo.UpdateAsync(id, request.RecurrenceType, request.DaysOfWeek, request.DayOfMonth,
                    request.TimeOfDay, request.Browser, request.LoginUserId, request.EndDate, nextRunDate);

                var changes = new List<AuditFieldChange>();
                if (!string.Equals(existing.RecurrenceType, request.RecurrenceType, StringComparison.Ordinal))
                    changes.Add(new AuditFieldChange { Field = "RecurrenceType", Old = existing.RecurrenceType, New = request.RecurrenceType });
                if (!string.Equals(existing.DaysOfWeek, request.DaysOfWeek, StringComparison.Ordinal))
                    changes.Add(new AuditFieldChange { Field = "DaysOfWeek", Old = existing.DaysOfWeek, New = request.DaysOfWeek });
                if (existing.DayOfMonth != request.DayOfMonth)
                    changes.Add(new AuditFieldChange { Field = "DayOfMonth", Old = existing.DayOfMonth, New = request.DayOfMonth });
                if (existing.TimeOfDay != request.TimeOfDay)
                    changes.Add(new AuditFieldChange { Field = "TimeOfDay", Old = existing.TimeOfDay.ToString(), New = request.TimeOfDay.ToString() });
                if (!string.Equals(existing.Browser, request.Browser, StringComparison.Ordinal))
                    changes.Add(new AuditFieldChange { Field = "Browser", Old = existing.Browser, New = request.Browser });
                if (existing.EndDate != request.EndDate)
                    changes.Add(new AuditFieldChange { Field = "EndDate", Old = existing.EndDate, New = request.EndDate });

                await _auditLog.LogAsync("RecurringSchedule", id, existing.AssignmentName, "Updated",
                    this.GetAuditUserId(), this.GetAuditUserName(), changes: changes);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recurring schedule {RecurringScheduleId}", id);
                return StatusCode(500, "An unexpected error occurred while updating the recurring schedule.");
            }
        }

        private static string ValidateRecurrence(RecurringScheduleRequest request)
        {
            if (request == null)
                return "Request body is required.";

            if (string.IsNullOrWhiteSpace(request.RecurrenceType) ||
                !new[] { "Daily", "Weekly", "Monthly" }.Contains(request.RecurrenceType))
                return "RecurrenceType must be 'Daily', 'Weekly', or 'Monthly'.";

            if (request.RecurrenceType == "Weekly" && string.IsNullOrWhiteSpace(request.DaysOfWeek))
                return "DaysOfWeek is required for a Weekly schedule.";

            if (request.RecurrenceType == "Monthly" && (request.DayOfMonth is null or < 1 or > 31))
                return "DayOfMonth (1-31) is required for a Monthly schedule.";

            if (string.IsNullOrWhiteSpace(request.Browser))
                return "Browser is required.";

            return null;
        }

        [HttpPost("{id:int}/pause")]
        public async Task<IActionResult> Pause(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            await _repo.SetActiveAsync(id, false);
            await _auditLog.LogAsync("RecurringSchedule", id, existing?.AssignmentName ?? $"Schedule #{id}", "Paused",
                this.GetAuditUserId(), this.GetAuditUserName());
            return Ok();
        }

        [HttpPost("{id:int}/resume")]
        public async Task<IActionResult> Resume(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            await _repo.SetActiveAsync(id, true);
            await _auditLog.LogAsync("RecurringSchedule", id, existing?.AssignmentName ?? $"Schedule #{id}", "Resumed",
                this.GetAuditUserId(), this.GetAuditUserName());
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            await _repo.DeleteAsync(id);
            await _auditLog.LogAsync("RecurringSchedule", id, existing?.AssignmentName ?? $"Schedule #{id}", "Deleted",
                this.GetAuditUserId(), this.GetAuditUserName(),
                snapshot: new { assignmentName = existing?.AssignmentName, recurrenceType = existing?.RecurrenceType });
            return Ok();
        }

        [HttpGet("{id:int}/history")]
        public async Task<IActionResult> GetHistory(int id)
        {
            var history = await _repo.GetRunHistoryAsync(id);
            return Ok(history);
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
