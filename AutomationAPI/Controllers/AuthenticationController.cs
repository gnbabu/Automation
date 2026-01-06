using AutomationAPI.Repositories;
using AutomationAPI.Repositories.Helpers;
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
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthenticationController(IAuthService authService, ILogger<AuthenticationController> logger, IEmailService emailService, IUserRepository userRepository, IConfiguration configuration)
        {
            _authService = authService;
            _logger = logger;
            _emailService = emailService;
            _userRepository = userRepository;
            _configuration = configuration;
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


        [HttpGet("test-email")]
        public async Task<IActionResult> TestEmail()
        {
            await _emailService.SendAsync(
            "naresh.net2009@gmail.com",
              "Forgot Username – OHPNM Automation Portal",
            "<b>SendGrid email working successfully 🚀</b>");

            return Ok("Email sent successfully");
        }

        [HttpPost("forgot-username")]
        public async Task<IActionResult> ForgotUsername([FromBody] ForgotUsernameRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email is required");

            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "No account found with the provided email address."
                });
            }

            var htmlBody = EmailTemplates.BuildForgotUsernameEmail(user.UserName);

            await _emailService.SendAsync(
                user.Email,
                "Forgot Username – OHPNM Automation Portal",
                htmlBody
            );

            return Ok(new
            {
                message = "Username has been sent to your registered email."
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Email is required." });

            var token = Guid.NewGuid().ToString("N");
            var expiry = DateTime.UtcNow.AddMinutes(30);

            var success = await _userRepository.ForgotPasswordAsync(
                request.Email,
                token,
                expiry
            );

            if (!success)
            {
                return NotFound(new { message = "No account found with this email." });
            }

            var baseUrl = _configuration["App:FrontendUrl"]!.TrimEnd('/');
            var resetLink = $"{baseUrl}/reset-password?token={token}";

            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            
            var html = EmailTemplates.ResetPassword(resetLink, user.UserName);

            await _emailService.SendAsync(
                request.Email,
                "Reset your password – OHPNM Automation Portal",
                html
            );

            return Ok(new { message = "Password reset link sent successfully." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Invalid request." });
            }

            // 🔐 Hash password ONCE
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            var success = await _userRepository.ResetPasswordAsync(
                request.Token,
                passwordHash
            );

            if (!success)
            {
                return BadRequest(new { message = "Invalid or expired reset token." });
            }

            return Ok(new { message = "Password reset successfully." });
        }

    }
}
