using System.Security.Claims;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EnvironmentController : ControllerBase
    {
        private readonly IEnvironmentRepository _repo;
        private readonly IAuditLogService _auditLog;
        private readonly ILogger<EnvironmentController> _logger;

        public EnvironmentController(IEnvironmentRepository repo, IAuditLogService auditLog, ILogger<EnvironmentController> logger)
        {
            _repo = repo;
            _auditLog = auditLog;
            _logger = logger;
        }

        // GET: api/environments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _repo.GetAllAsync());
        }

        // GET: api/environments/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid Environment Id");

            var env = await _repo.GetByIdAsync(id);
            return env == null ? NotFound() : Ok(env);
        }

        // POST: api/environments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EnvironmentRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int id;
            try
            {
                id = await _repo.CreateAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create environment {EnvironmentName}", request.EnvironmentName);
                return Conflict(GetUserMessage(ex, "An environment with that name already exists."));
            }

            await _auditLog.LogAsync("Environment", id, request.EnvironmentName, "Created",
                this.GetAuditUserId(), this.GetAuditUserName(),
                snapshot: new { environmentName = request.EnvironmentName, environmentUrl = request.EnvironmentUrl, requiresAuthentication = request.RequiresAuthentication });

            return Ok(new { EnvironmentId = id });
        }

        // PUT: api/environments
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] EnvironmentRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!request.EnvironmentId.HasValue || request.EnvironmentId <= 0)
                return BadRequest("EnvironmentId is required for update");

            var existing = await _repo.GetByIdAsync(request.EnvironmentId.Value);
            if (existing == null)
                return NotFound();

            request.ModifiedBy = GetCurrentUserId();

            try
            {
                await _repo.UpdateAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update environment {EnvironmentId}", request.EnvironmentId);
                return Conflict(GetUserMessage(ex, "An environment with that name already exists."));
            }

            var changes = new List<AuditFieldChange>();
            if (!string.Equals(existing.EnvironmentName, request.EnvironmentName, StringComparison.Ordinal))
                changes.Add(new AuditFieldChange { Field = "EnvironmentName", Old = existing.EnvironmentName, New = request.EnvironmentName });
            if (!string.Equals(existing.EnvironmentUrl, request.EnvironmentUrl, StringComparison.Ordinal))
                changes.Add(new AuditFieldChange { Field = "EnvironmentUrl", Old = existing.EnvironmentUrl, New = request.EnvironmentUrl });
            if (existing.RequiresAuthentication != (request.RequiresAuthentication ?? existing.RequiresAuthentication))
                changes.Add(new AuditFieldChange { Field = "RequiresAuthentication", Old = existing.RequiresAuthentication, New = request.RequiresAuthentication });
            if (!string.Equals(existing.Description, request.Description, StringComparison.Ordinal))
                changes.Add(new AuditFieldChange { Field = "Description", Old = existing.Description, New = request.Description });

            await _auditLog.LogAsync("Environment", request.EnvironmentId, request.EnvironmentName, "Updated",
                this.GetAuditUserId(), this.GetAuditUserName(), changes: changes);

            return Ok();
        }

        // SOFT DELETE (used by the list page's Disable action)
        [HttpDelete("{id:int}/soft")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid EnvironmentId");

            var env = await _repo.GetByIdAsync(id);
            if (env == null)
                return NotFound();

            await _repo.SoftDeleteAsync(id, GetCurrentUserId());

            await _auditLog.LogAsync("Environment", id, env.EnvironmentName, "Disabled",
                this.GetAuditUserId(), this.GetAuditUserName(),
                snapshot: new { environmentName = env.EnvironmentName });

            return Ok(new { Message = "Environment soft-deleted successfully" });
        }

        // HARD DELETE
        [HttpDelete("{id:int}/hard")]
        public async Task<IActionResult> HardDelete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid EnvironmentId");

            var env = await _repo.GetByIdAsync(id);
            if (env == null)
                return NotFound();

            try
            {
                await _repo.HardDeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Delete blocked for environment {EnvironmentId}", id);
                return Conflict(GetUserMessage(ex, "This environment could not be deleted."));
            }

            await _auditLog.LogAsync("Environment", id, env.EnvironmentName, "Deleted",
                this.GetAuditUserId(), this.GetAuditUserName(),
                snapshot: new { environmentName = env.EnvironmentName, environmentUrl = env.EnvironmentUrl });

            return Ok(new { Message = "Environment permanently deleted" });
        }

        // ---- helpers ----

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var userId) && userId > 0 ? userId : null;
        }

        private static string GetUserMessage(Exception ex, string fallback)
        {
            // SqlDataAccessHelper wraps SQL errors; surface the innermost RAISERROR text.
            var inner = ex;
            while (inner.InnerException != null)
                inner = inner.InnerException;
            return string.IsNullOrWhiteSpace(inner.Message) ? fallback : inner.Message;
        }
    }
}
