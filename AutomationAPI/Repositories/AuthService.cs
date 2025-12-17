using AutomationAPI.Controllers;
using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AutomationAPI.Repositories
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        public AuthService(IConfiguration configuration,
                           IUserRepository userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
        }

        public async Task<AuthResponse> Login(LoginModel model)
        {
            try
            {
                // Validate user credentials
                var user = await _userRepository.GetUserByUsernameAsync(model.Username);

                // Check if user exists
                if (user == null)
                {
                    return new AuthResponse { StatusCode = 0, Message = "Invalid username or password" };
                }

                // Check if the user is active
                if (!user.Active)
                {
                    return new AuthResponse { StatusCode = 0, Message = "User is inactive" };
                }

                bool isPasswordValid = false;

                // ✅ NEW USERS (BCrypt)
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(
                        model.Password,
                        user.PasswordHash
                    );
                }

                if (!isPasswordValid)
                {
                    return new AuthResponse
                    {
                        StatusCode = 0,
                        Message = "Invalid username or password"
                    };
                }

                await _userRepository.UpdateLastLoginAsync(user.UserId);

                // Create the claims for the JWT token
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, user.RoleName)
                };

                // Generate the token
                var token = GenerateToken(authClaims);

                // Return successful response with user and token
                return new AuthResponse { StatusCode = 1, Message = "Login successful", User = user, Token = token };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user {Username}", model.Username);
                return new AuthResponse { StatusCode = 0, Message = "An error occurred during login" };
            }
        }


        private string GenerateToken(IEnumerable<Claim> claims)
        {
            //To DO Expire token should be changed
            var secretKey = _configuration["JWTKey:Secret"] ?? throw new InvalidOperationException("JWT secret key not configured.");

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var _TokenExpiryTimeInHour = Convert.ToInt64(_configuration["JWTKey:TokenExpiryTimeInHour"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["JWTKey:ValidIssuer"],
                Audience = _configuration["JWTKey:ValidAudience"],
                Expires = DateTime.UtcNow.AddHours(_TokenExpiryTimeInHour),
                //Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(claims)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public async Task<bool> Register(RegistrationModel model)
        {

            if (string.IsNullOrWhiteSpace(model.Username) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.ConfirmPassword))
                throw new ArgumentException("All fields are required.");

            if (model.Password != model.ConfirmPassword)
                throw new ArgumentException("Passwords do not match.");

            if (!IsValidPassword(model.Password))
                throw new ArgumentException("Password does not meet complexity requirements.");

            var users = await _userRepository.GetAllUsersAsync();
            if (users.Any(u => u.UserName.Equals(model.Username, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Username already exists.");

            if (users.Any(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Email already registered.");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            model.Password = passwordHash;

            var userId = await _userRepository.RegisterUserAsync(model);

            return userId > 0;
        }

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < 8)
                return false;

            var hasUpper = Regex.IsMatch(password, "[A-Z]");
            var hasLower = Regex.IsMatch(password, "[a-z]");
            var hasDigit = Regex.IsMatch(password, "[0-9]");
            var hasSpecial = Regex.IsMatch(password, "[^a-zA-Z0-9]");

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

    }
}
