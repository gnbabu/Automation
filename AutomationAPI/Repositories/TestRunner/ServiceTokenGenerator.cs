using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AutomationAPI.Repositories.TestRunner
{
    // Mints a short-lived JWT for the isolated test process to call back into
    // AutomationAPI (e.g. Selenium.BaseComponents.Utilities.APIGatway.GetAutomationData
    // hitting the [Authorize]-protected api/Automation/data/flow/{flowName}) - the test
    // process has no HTTP request/user session of its own to reuse a real user's token
    // from, so TestQueueWorker mints one of these instead, threaded in via TestParameters
    // the same way Browser already is. Signed with the same JWTKey secret real user logins
    // use (AuthService.GenerateToken), so AutomationController's [Authorize] accepts it
    // without any server-side changes. Scoped to a generic "TestRunner" identity, not a
    // specific user, and kept short-lived (a handful of minutes - long enough to cover one
    // queued test run, not reusable afterward).
    public class ServiceTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public ServiceTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateTestRunnerToken(TimeSpan? lifetime = null)
        {
            var secretKey = _configuration["JWTKey:Secret"]
                ?? throw new InvalidOperationException("JWT secret key not configured.");

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestRunner"),
                new Claim(ClaimTypes.NameIdentifier, "0"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["JWTKey:ValidIssuer"],
                Audience = _configuration["JWTKey:ValidAudience"],
                Expires = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromMinutes(30)),
                SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(claims)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
