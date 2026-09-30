using System.ComponentModel.DataAnnotations;

namespace AutomationAPI.Repositories.Models
{
    public class EnvironmentModel
    {
        public int EnvironmentId { get; set; }
        public string EnvironmentName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string? EnvironmentUrl { get; set; }
        public bool RequiresAuthentication { get; set; } = true;

        // Third-party/SSO authentication in production - no login screen for this
        // environment at all. Only meaningful when RequiresAuthentication is true; treated
        // the same as RequiresAuthentication = false everywhere a Login User is normally
        // needed (Run Now/Schedule dialogs, Recurring Schedule form, and Selenium's own
        // login step).
        public bool EnableSso { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? ModifiedByName { get; set; }
        public int ReleaseCount { get; set; }
    }



    public class EnvironmentRequestDto
    {
        public int? EnvironmentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EnvironmentName { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string? EnvironmentUrl { get; set; }

        public bool? RequiresAuthentication { get; set; }

        public bool? EnableSso { get; set; }

        [Required]
        public int CreatedBy { get; set; }   // FK → aut.User(UserID)

        public bool? IsActive { get; set; }

        public int? ModifiedBy { get; set; }
    }

}
