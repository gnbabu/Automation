using System.Security.Claims;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LoginUserController : ControllerBase
    {
        private readonly ILoginUserRepository _repo;
        private readonly ILogger<LoginUserController> _logger;

        public LoginUserController(ILoginUserRepository repo, ILogger<LoginUserController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        // GET: api/LoginUser/environment/{environmentId}
        // Unfiltered - every login user for the environment, any owner. No longer
        // called by the Portal's own UI after the self-service change (Run Now/Schedule
        // and the management screen both use the ownership-filtered endpoints below
        // instead) - left in place/unused rather than removed (see AGENTS.md).
        [HttpGet("environment/{environmentId:int}")]
        public async Task<IActionResult> GetByEnvironment(int environmentId)
        {
            if (environmentId <= 0)
                return BadRequest("Invalid EnvironmentId");

            return Ok(await _repo.GetByEnvironmentAsync(environmentId));
        }

        // GET: api/LoginUser/environment/{environmentId}/mine
        // Self-service: only the caller's own login user(s) for this environment. Used
        // by the Credential Configuration screen and by Run Now/Schedule's dropdown
        // resolution - every user manages/selects only their own credential, never
        // someone else's (see AGENTS.md).
        [HttpGet("environment/{environmentId:int}/mine")]
        public async Task<IActionResult> GetMineForEnvironment(int environmentId)
        {
            if (environmentId <= 0)
                return BadRequest("Invalid EnvironmentId");

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized();

            return Ok(await _repo.GetByEnvironmentAndPortalUserAsync(environmentId, currentUserId.Value));
        }

        // POST: api/LoginUser
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LoginUserRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Password is required when creating a login user.");

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized();

            // Self-service: a credential is always created for the caller themselves,
            // regardless of anything the client sends - never trust the client for this.
            request.PortalUserId = currentUserId;
            request.CreatedBy = currentUserId;

            int id;
            try
            {
                id = await _repo.CreateAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create login user for environment {EnvironmentId}", request.EnvironmentId);
                return Conflict(GetUserMessage(ex, "Failed to create login user."));
            }

            return Ok(new { LoginUserId = id });
        }

        // PUT: api/LoginUser
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] LoginUserRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!request.LoginUserId.HasValue || request.LoginUserId <= 0)
                return BadRequest("LoginUserId is required for update");

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized();

            request.ModifiedBy = currentUserId;

            bool updated;
            try
            {
                updated = await _repo.UpdateAsync(request, currentUserId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update login user {LoginUserId}", request.LoginUserId);
                return Conflict(GetUserMessage(ex, "Failed to update login user."));
            }

            if (!updated)
                return Forbid();

            return Ok();
        }

        // DELETE: api/LoginUser/{id}/soft
        [HttpDelete("{id:int}/soft")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid LoginUserId");

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized();

            var disabled = await _repo.SoftDeleteAsync(id, currentUserId.Value, currentUserId);
            if (!disabled)
                return Forbid();

            return Ok(new { Message = "Login user disabled successfully" });
        }

        // DELETE: api/LoginUser/{id}/hard
        [HttpDelete("{id:int}/hard")]
        public async Task<IActionResult> HardDelete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid LoginUserId");

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized();

            bool deleted;
            try
            {
                deleted = await _repo.HardDeleteAsync(id, currentUserId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Delete blocked for login user {LoginUserId}", id);
                return Conflict(GetUserMessage(ex, "This login user could not be deleted."));
            }

            if (!deleted)
                return Forbid();

            return Ok(new { Message = "Login user permanently deleted" });
        }

        // GET: api/LoginUser/{id}/credentials
        // The only endpoint that ever returns a decrypted password - only ever called by
        // an isolated test process's service JWT (ServiceTokenGenerator mints tokens with
        // NameIdentifier "0"/Name "TestRunner"), never by the Portal's own regular UI
        // flows even though those flows also carry a valid, [Authorize]-satisfying token.
        [HttpGet("{id:int}/credentials")]
        public async Task<IActionResult> GetCredentials(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid LoginUserId");

            if (!IsServiceToken())
                return Forbid();

            var credentials = await _repo.GetCredentialsAsync(id);
            return credentials == null ? NotFound() : Ok(credentials);
        }

        // GET: api/LoginUser/resolve?environmentId={id}&role={role}
        // Used only by BaseFeatureFixture.LoginByProfile's mid-test role-switch (e.g.
        // TC.Registration logging in as a different role partway through a test) - a
        // genuinely different, unattended use case from the initial Run Now/Schedule
        // login. Same service-token-only protection as GetCredentials, since this also
        // returns a decrypted password.
        [HttpGet("resolve")]
        public async Task<IActionResult> ResolveByRole([FromQuery] int environmentId, [FromQuery] string role)
        {
            if (environmentId <= 0 || string.IsNullOrWhiteSpace(role))
                return BadRequest("environmentId and role are required");

            if (!IsServiceToken())
                return Forbid();

            var credentials = await _repo.ResolveByRoleAsync(environmentId, role);
            return credentials == null ? NotFound() : Ok(credentials);
        }

        // ---- helpers ----

        private bool IsServiceToken()
        {
            var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var name = User.FindFirstValue(ClaimTypes.Name);
            return nameIdentifier == "0" && name == "TestRunner";
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var userId) && userId > 0 ? userId : null;
        }

        private static string GetUserMessage(Exception ex, string fallback)
        {
            var inner = ex;
            while (inner.InnerException != null)
                inner = inner.InnerException;
            return string.IsNullOrWhiteSpace(inner.Message) ? fallback : inner.Message;
        }
    }
}
