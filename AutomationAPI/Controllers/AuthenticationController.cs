using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Mvc;

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
                _logger.LogError(ex, "Login failed for user {Email}", model.Username);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }



        [HttpPost]
        [Route("forgotpassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Invalid payload");

                var response = await _authService.ForgotPassword(email);

                if (response == false)
                    return BadRequest("Invalid data");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid email {Email}", email);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                _logger.LogWarning("Invalid registration payload: {Errors}", string.Join("; ", errors));

                return BadRequest(new { result = false, message = "Invalid registration data.", errors });
            }

            try
            {
                var isRegistered = await _authService.Register(model);

                if (!isRegistered)
                {
                    _logger.LogWarning("Registration failed for user: {Username}", model.Username);

                    return BadRequest(new { result = false, message = "Registration failed. Invalid or incomplete details." });
                }

                return Ok(new { result = true, message = "Registration successful! Please check your email." });
            }
            catch (ArgumentException ex)
            {

                _logger.LogWarning(ex, "Validation error during registration");

                return BadRequest(new
                {
                    result = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration");

                return StatusCode(500, new { result = false, message = "An unexpected error occurred during registration." });
            }
        }


    }
}
