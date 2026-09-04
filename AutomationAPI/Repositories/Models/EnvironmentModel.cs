using System.ComponentModel.DataAnnotations;

namespace AutomationAPI.Repositories.Models
{
    public class EnvironmentModel
    {
        public int EnvironmentId { get; set; }
        public string EnvironmentName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        // Joined User info
        public string UserName { get; set; }
        public string Email { get; set; }

        // Who last edited/disabled this environment (nullable - never modified since
        // creation, or created before ModifiedBy tracking was added).
        public string? ModifiedByName { get; set; }

        // How many Releases currently use this environment - filesystem/DB-derived,
        // used by the frontend to show "N releases use this environment" and to gate the
        // Delete button (mirrors the guard already enforced server-side in
        // usp_EnvironmentHardDelete).
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

        [Required]
        public int CreatedBy { get; set; }   // FK → aut.User(UserID)

        public bool? IsActive { get; set; }

        // Acting user for Update/SoftDelete calls - who made this change, not who
        // originally created the environment (CreatedBy above is unrelated/unused on
        // update - the SP never touches it).
        public int? ModifiedBy { get; set; }
    }

}
