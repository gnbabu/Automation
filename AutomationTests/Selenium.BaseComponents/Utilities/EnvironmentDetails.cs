namespace Selenium.BaseComponents.Utilities
{
    // Mirrors AutomationAPI.Repositories.Models.EnvironmentModel's shape (just the fields
    // needed here) - returned by GET api/Environment/{id}.
    public class EnvironmentDetails
    {
        public int EnvironmentId { get; set; }
        public string? EnvironmentName { get; set; }
        public string? EnvironmentUrl { get; set; }
        public bool RequiresAuthentication { get; set; } = true;

        // Third-party/SSO authentication in production - no login screen to automate
        // against. Treated the same as RequiresAuthentication = false by
        // BaseFeatureFixture's login step.
        public bool EnableSso { get; set; }
    }

    // Mirrors AutomationAPI.Repositories.Models.LoginUserCredentials - returned only by
    // GET api/LoginUser/{id}/credentials (the only endpoint that ever exposes a
    // decrypted password).
    public class LoginUserCredentials
    {
        public int LoginUserId { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}
