using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(IAuthService authService, ILogger<AuthenticationController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Invalid payload");

                var response = await _authService.Login(model);

                if (response.StatusCode == 0)
                    return BadRequest(response.Message);

                return Ok(new
                {
                    message = response.Message,
                    token = response.Token,
                    user = response.User
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user {Email}", model.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
