using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                return StatusCode(500, "An error occurred while fetching users.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching user with ID {id}");
                return StatusCode(500, "An error occurred while fetching the user.");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            try
            {
                var userId = await _userRepository.CreateUserAsync(user);
                return CreatedAtAction(nameof(GetUserById), new { id = userId }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, "An error occurred while creating the user.");
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser([FromBody] User user)
        {
            try
            {
                await _userRepository.UpdateUserAsync(user);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return StatusCode(500, "An error occurred while updating the user.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userRepository.DeleteUserAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID {id}");
                return StatusCode(500, "An error occurred while deleting the user.");
            }
        }

        // Self-service profile update - no role restriction (any authenticated user may
        // update their own profile). UserId always comes from the JWT's NameIdentifier
        // claim (added at login in AuthService.cs), never from the request body -
        // UpdateOwnProfileRequest has no UserId field at all, so there's no way to target
        // another user's row through this endpoint. Only Photo/PhoneNumber/TimeZone can
        // be changed here; Role/Status/Priority/Active/Teams stay Admin-only (UpdateUser).
        [HttpPut("me/profile")]
        public async Task<IActionResult> UpdateOwnProfile([FromBody] UpdateOwnProfileRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim, out var userId) || userId <= 0)
                    return Unauthorized("Unable to determine the current user.");

                await _userRepository.UpdateOwnProfileAsync(userId, request.Photo, request.PhoneNumber, request.TimeZone);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating own profile");
                return StatusCode(500, "An error occurred while updating your profile.");
            }
        }

        [HttpPost("Filters")]
        public async Task<IActionResult> GetUsers([FromBody] UserFilter filters)
        {
            var users = await _userRepository.GetFilteredUsersAsync(filters);
            return Ok(users);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (request == null ||
                    string.IsNullOrWhiteSpace(request.OldPassword) ||
                    string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return BadRequest(new { message = "Invalid request." });
                }
                await _userRepository.ChangePasswordAsync(request);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Error changing password for user: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, $"Error changing password for user");
                return StatusCode(500, "An error occurred while resetting the password.");
            }
        }

        [HttpPost("activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetUserActiveStatus([FromBody] SetActiveStatusRequest request)
        {
            try
            {
                await _userRepository.SetUserActiveStatusAsync(request.UserId, request.Active);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating active status for user {request.UserId}");
                return StatusCode(500, "An error occurred while updating user status.");
            }
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetUserRolesAsync()
        {
            try
            {
                var userRoles = await _userRepository.GetUserRolesAsync();
                return Ok(userRoles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user roles");
                return StatusCode(500, "An error occurred while fetching user roles.");
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetUserStatusAsync()
        {
            try
            {
                var status = await _userRepository.GetUserStatusesAsync();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user status.");
                return StatusCode(500, "An error occurred while fetching user status.");
            }
        }

        [HttpGet("timezones")]
        public async Task<IActionResult> GetTimeZonesAsync()
        {
            try
            {
                var zones = await _userRepository.GetTimeZonesAsync();
                return Ok(zones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching time zones.");
                return StatusCode(500, "An error occurred while fetching time zones.");
            }
        }

        [HttpGet("priorities")]
        public async Task<IActionResult> GetPriorityStatusAsync()
        {
            try
            {
                var priorities = await _userRepository.GetPriorityStatusesAsync();
                return Ok(priorities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching priority status.");
                return StatusCode(500, "An error occurred while fetching priority status.");
            }
        }


    }
}
