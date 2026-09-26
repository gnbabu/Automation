using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    // Admin-only, matching the frontend's adminGuard convention already used for
    // User Management/Environment Management - the activity log surfaces every user's
    // actions across the whole app, not just the caller's own.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _service;
        private readonly ILogger<AuditLogController> _logger;

        public AuditLogController(IAuditLogService service, ILogger<AuditLogController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET: api/AuditLog?entityType=&entityId=&actorUserId=&action=&fromDate=&toDate=&pageNumber=&pageSize=
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] AuditLogFilter filter)
        {
            try
            {
                var result = await _service.GetPagedAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching audit log");
                return StatusCode(500, "An error occurred while fetching the activity log.");
            }
        }

        // GET: api/AuditLog/entity-types - powers the Entity Type filter dropdown with
        // exactly what's actually been logged so far, rather than a hard-coded list.
        [HttpGet("entity-types")]
        public async Task<IActionResult> GetEntityTypes()
        {
            var types = await _service.GetDistinctEntityTypesAsync();
            return Ok(types);
        }
    }
}
