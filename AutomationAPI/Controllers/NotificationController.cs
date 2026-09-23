using System.Security.Claims;
using AutomationAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _repo;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationRepository repo, ILogger<NotificationController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        // GET: api/Notification?unreadOnly=false
        // Always scoped to the caller - there is no "view another user's notifications"
        // endpoint, matching how every recipient's feed is strictly personal.
        [HttpGet]
        public async Task<IActionResult> GetMine([FromQuery] bool unreadOnly = false)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var notifications = await _repo.GetByUserAsync(userId.Value, unreadOnly);
            return Ok(notifications);
        }

        // GET: api/Notification/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var count = await _repo.GetUnreadCountAsync(userId.Value);
            return Ok(new { count });
        }

        // POST: api/Notification/{id}/mark-read
        [HttpPost("{id:int}/mark-read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            await _repo.MarkReadAsync(id, userId.Value);
            return Ok();
        }

        // POST: api/Notification/mark-all-read
        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            await _repo.MarkAllReadAsync(userId.Value);
            return Ok();
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var userId) && userId > 0 ? userId : null;
        }
    }
}
