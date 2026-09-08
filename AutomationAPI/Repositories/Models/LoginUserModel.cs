using System.ComponentModel.DataAnnotations;

namespace AutomationAPI.Repositories.Models
{
    // Never carries a password - used for the management screen list and the Run
    // Now/Schedule dropdown alike (GET api/LoginUser/environment/{id}).
    public class LoginUserModel
    {
        public int LoginUserId { get; set; }
        public int EnvironmentId { get; set; }

        // Optional label: "whose credential is this" - not a matching key.
        public int? PortalUserId { get; set; }
        public string? PortalUserName { get; set; }

        // Free text label, e.g. "TechAdmin", "CredSpec" - matches whatever string a
        // test's own [TestFixture("...")] declares. Not enforced/matched automatically;
        // shown so the person picking a login user at Run Now/Schedule time can tell
        // which credential is meant for which role.
        public string UserRole { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class LoginUserRequestDto
    {
        public int? LoginUserId { get; set; }

        [Required]
        public int EnvironmentId { get; set; }

        public int? PortalUserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserRole { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        // Plaintext in the request only - encrypted immediately server-side before
        // storage. Optional on update: leaving it blank keeps the existing password
        // unchanged (the UI never pre-fills/shows an existing password to re-submit).
        public string? Password { get; set; }

        public bool? IsActive { get; set; }

        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }

    // Returned only by GET api/LoginUser/{id}/credentials - the only endpoint that ever
    // exposes a decrypted password.
    public class LoginUserCredentials
    {
        public int LoginUserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
