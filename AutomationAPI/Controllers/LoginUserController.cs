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
        // Used by both the Login Users management screen and the Run Now/Schedule
        // dropdowns. Never returns a password.
        [HttpGet("environment/{environmentId:int}")]
        public async Task<IActionResult> GetByEnvironment(int environmentId)
        {
            if (environmentId <= 0)
                return BadRequest("Invalid EnvironmentId");

            return Ok(await _repo.GetByEnvironmentAsync(environmentId));
        }

        // POST: api/LoginUser
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LoginUserRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Password is required when creating a login user.");

            request.CreatedBy = GetCurrentUserId();

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

            request.ModifiedBy = GetCurrentUserId();

            try
            {
                await _repo.UpdateAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update login user {LoginUserId}", request.LoginUserId);
                return Conflict(GetUserMessage(ex, "Failed to update login user."));
            }

            return Ok();
        }

        // DELETE: api/LoginUser/{id}/soft
        [HttpDelete("{id:int}/soft")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid LoginUserId");

            await _repo.SoftDeleteAsync(id, GetCurrentUserId());
            return Ok(new { Message = "Login user disabled successfully" });
        }

        // DELETE: api/LoginUser/{id}/hard
        [HttpDelete("{id:int}/hard")]
        public async Task<IActionResult> HardDelete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid LoginUserId");

            try
            {
                await _repo.HardDeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Delete blocked for login user {LoginUserId}", id);
                return Conflict(GetUserMessage(ex, "This login user could not be deleted."));
            }

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
